namespace FStock.Analysis.V1

open System
open System.Diagnostics
open FStock.Analysis.V1.Core
open FStock.Analysis.V1.Core.Persistence
open FStock.Analysis.V1.TechnicalIndicators
open FStock.Data.Store
open Freql.Sqlite
open Microsoft.FSharp.Core

module Store =

    type BuildSettings =
        { Sampling: SamplingType
          BuildMode: BuildMode
          TechnicalIndicators: TechnicalIndicator list }

    and [<RequireQualifiedAccess>] SamplingType =
        | None
        | Fixed of int
        | Percentage of float

    and [<RequireQualifiedAccess>] BuildMode =
        | Overwrite
        | Append
        | SkipIfExists

    and TechnicalIndicator =
        | Sma of TableName: string * Parameters: SimpleMovingAverage.Parameters
        | Ema of TableName: string * Parameters: ExponentialMovingAverage.Parameters
        | Rsi of TableName: string * Parameters: RelativeStrengthIndex.Parameters
        | Macd of TableName: string * Parameters: MovingAverageConvergenceDivergence.Parameters

        member ti.GetName() =
            match ti with
            | Sma(tableName, parameters) -> "sma"
            | Ema(tableName, parameters) -> "ema"
            | Rsi(tableName, parameters) -> "rsi"
            | Macd(tableName, parameters) -> "macd"

        member ti.GetLookBack() =
            match ti with
            | Sma(tableName, parameters) -> parameters.WindowSize
            | Ema(tableName, parameters) -> parameters.WindowSize
            | Rsi(tableName, parameters) -> parameters.Periods
            | Macd(tableName, parameters) ->
                [ parameters.SignalPeriods
                  parameters.ShortTermPeriods
                  parameters.LongTermPeriods ]
                |> List.max

    let initialize (ctx: SqliteContext) =
        [ Records.Artifact.CreateTableSql()
          Records.Stock.CreateTableSql()
          Records.MetadataItem.CreateTableSql()
          Records.StockEntry.CreateTableSql()
          Records.TableListingItem.CreateTableSql() ]
        |> List.iter (ctx.ExecuteSqlNonQuery >> ignore)

    let createTable (ctx: SqliteContext) (technicalIndicator: TechnicalIndicator) =
        match technicalIndicator with
        | Sma(tableName, parameters) ->

            ctx.CreateTable<SimpleMovingAverage.SmaItem>(tableName) |> ignore
        // Add the table listings

        | Ema(tableName, parameters) ->

            ctx.CreateTable<ExponentialMovingAverage.EmaItem>(tableName) |> ignore
        // Add the table listings

        | Rsi(tableName, parameters) ->

            ctx.CreateTable<RelativeStrengthIndex.RsiItem>(tableName) |> ignore
        // Add the table listings

        | Macd(tableName, parameters) ->

            ctx.CreateTable<MovingAverageConvergenceDivergence.MacdItem>(tableName)
            |> ignore
    // Add the table listings


    let build (settings: BuildSettings) (store: FStock.Data.Store.FStockStore) (path: string) (start: DateTime) =

        let sw = Stopwatch()
        sw.Start()
        
        match settings.BuildMode with
        | BuildMode.Overwrite ->
            use ctx = SqliteContext.Create path

            initialize ctx

            settings.TechnicalIndicators |> List.iter (createTable ctx)

            let stocks =
                match settings.Sampling with
                | SamplingType.None ->
                    let metadata = store.GetAllStockMetaData()

                    printfn $"{metadata.Length} symbols found"


                    metadata
                    |> List.iteri (fun i md ->
                        // Get the lock back stock data
                        printfn $"Handling {md.Symbol} - {md.SecurityName} ({i + 1}/{metadata.Length})"

                        Operations.insertStock
                            ctx
                            { Symbol = md.Symbol
                              Name = md.SecurityName }

                        let firstQuery =
                            ({ From = start.AddMonths(-3) |> Some
                               FromInclusive = true
                               To = Some start
                               ToInclusive = true
                               SymbolFilter = Queries.SymbolFilter.EqualTo md.Symbol }
                            : Queries.EntryQuery)

                        let baseData = store.ExecuteStockQuery(firstQuery)

                        match baseData.IsEmpty with
                        | true -> ()
                        | false ->

                            let auxData =
                                store.GetPreviousXStockEntries(
                                    md.Symbol,
                                    baseData |> List.minBy (fun e -> e.EntryDate) |> (fun e -> e.EntryDate),
                                    200
                                )

                            let allData = baseData @ auxData |> List.sortBy (fun d -> d.EntryDate)

                            allData
                            |> List.iter (fun d ->
                                Operations.insertStockEntry
                                    ctx
                                    { Symbol = d.Symbol
                                      EntryDate = d.EntryDate
                                      OpenValue = d.OpenValue
                                      HighValue = d.HighValue
                                      LowValue = d.LowValue
                                      CloseValue = d.CloseValue
                                      AdjustedCloseValue = d.AdjustedCloseValue
                                      VolumeValue = d.VolumeValue })

                            let data =
                                allData
                                |> List.sortBy (fun d -> d.EntryDate)
                                |> List.map (fun d ->
                                    { Symbol = d.Symbol
                                      Date = d.EntryDate
                                      Price = d.CloseValue }
                                    : BasicInputItem)

                            let r =
                                ctx.ExecuteInTransaction(fun t ->
                                    settings.TechnicalIndicators
                                    |> List.iter (fun ti ->
                                        match ti with
                                        | Sma(tableName, parameters) ->
                                            t.InsertList(tableName, data |> SimpleMovingAverage.generate parameters)
                                        | Ema(tableName, parameters) ->
                                            t.InsertList(
                                                tableName,
                                                data |> ExponentialMovingAverage.generate parameters
                                            )
                                        | Rsi(tableName, parameters) ->
                                            t.InsertList(
                                                tableName,
                                                data |> RelativeStrengthIndex.generate parameters
                                            )
                                        | Macd(tableName, parameters) ->
                                            t.InsertList(
                                                tableName,
                                                data |> MovingAverageConvergenceDivergence.generate parameters
                                            )))

                            match r with
                            | Ok _ -> ()
                            | Error e -> printfn $"*** Error - {e}")

                    ()
                | SamplingType.Fixed i -> failwith "todo"
                | SamplingType.Percentage f -> failwith "todo"

            ()
        | BuildMode.Append -> failwith "todo"
        | BuildMode.SkipIfExists -> failwith "todo"

        sw.Stop()
        printfn $"Time taken: {sw.Elapsed.TotalMinutes} min(s)"