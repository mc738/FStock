namespace FStock.Analysis.V1

open System
open System.IO
open FStock.Analysis.V1
open FStock.Analysis.V1.Persistence
open FStock.Data
open Freql.Sqlite

[<AutoOpen>]
module Impl =

    type BuildParameters =
        { SampleSize: float
          Date: DateTime
          LookBackCount: int
          Path: string }

    let initialize (ctx: SqliteContext) =
        [ Records.Stock.CreateTableSql()
          Records.DayValue.CreateTableSql()
          Records.MetadataItem.CreateTableSql() ]
        |> List.iter (ctx.ExecuteSqlNonQuery >> ignore)

        ctx


    /// <summary>
    /// Build a point-in-time analytics store.
    /// This is essentially a store of analytical data based on a specific day.
    /// </summary>
    /// <param name="storeCtx"></param>
    /// <param name="parameters"></param>
    let build (storeCtx: SqliteContext) (parameters: BuildParameters) =
        let ctx =
            match File.Exists parameters.Path with
            | true -> SqliteContext.Open parameters.Path
            | false -> SqliteContext.Create parameters.Path |> initialize

        // Get (sampled) stock records for day.

        let allMetadata = Store.getAllStockMetadata storeCtx

        printfn $"Fetched {allMetadata.Length} items"


        allMetadata
        |> List.mapi (fun i md ->
            ctx.ExecuteInTransaction(fun t ->
                printfn $"Handling {md.Symbol} ({i + 1}/{allMetadata.Length})"

                ({ Symbol = md.Symbol
                   Name = md.SecurityName }
                : Parameters.NewStock)
                |> Operations.insertStock t

                Store.previousXStockEntriesInclusive storeCtx parameters.LookBackCount parameters.Date md.Symbol
                |> List.map (fun se ->

                    let ``7 days`` =
                        History.getMovingAverages storeCtx md.Symbol se.EntryDate 7 |> List.head

                    let ``20 days`` =
                        History.getMovingAverages storeCtx md.Symbol se.EntryDate 20 |> List.head

                    let ``50 days`` =
                        History.getMovingAverages storeCtx md.Symbol se.EntryDate 50 |> List.head

                    let ``100 days`` =
                        History.getMovingAverages storeCtx md.Symbol se.EntryDate 100 |> List.head

                    let ``200 days`` =
                        History.getMovingAverages storeCtx md.Symbol se.EntryDate 200 |> List.head

                    let ``252 days`` =
                        History.getMovingAverages storeCtx md.Symbol se.EntryDate 252 |> List.head


                    ({ Symbol = md.Symbol
                       EntryDate = se.EntryDate
                       OpenValue = se.OpenValue
                       HighValue = se.HighValue
                       LowValue = se.LowValue
                       CloseValue = se.CloseValue
                       AdjCloseValue = se.AdjustedCloseValue
                       VolumeValue = se.VolumeValue
                       MaPrev7OpenValue = ``7 days``.Open
                       MaPrev7HighValue = ``7 days``.High
                       MaPrev7LowValue = ``7 days``.Low
                       MaPrev7CloseValue = ``7 days``.Close
                       MaPrev7AdjCloseValue = ``7 days``.Close
                       MaPrev7VolumeValue = ``7 days``.Volume
                       MaPrev20OpenValue = ``20 days``.Open
                       MaPrev20HighValue = ``20 days``.High
                       MaPrev20LowValue = ``20 days``.Low
                       MaPrev20CloseValue = ``20 days``.Close
                       MaPrev20AdjCloseValue = ``20 days``.AdjustedClose
                       MaPrev20VolumeValue = ``20 days``.Volume
                       MaPrev50OpenValue = ``50 days``.Open
                       MaPrev50HighValue = ``50 days``.High
                       MaPrev50LowValue = ``50 days``.Low
                       MaPrev50CloseValue = ``50 days``.Close
                       MaPrev50AdjCloseValue = ``50 days``.AdjustedClose
                       MaPrev50VolumeValue = ``50 days``.Volume
                       MaPrev100OpenValue = ``100 days``.Open
                       MaPrev100HighValue = ``100 days``.High
                       MaPrev100LowValue = ``100 days``.Low
                       MaPrev100CloseValue = ``100 days``.Close
                       MaPrev100AdjCloseValue = ``100 days``.AdjustedClose
                       MaPrev100VolumeValue = ``100 days``.Volume
                       MaPrev200OpenValue = ``200 days``.Open
                       MaPrev200HighValue = ``200 days``.High
                       MaPrev200LowValue = ``200 days``.Low
                       MaPrev200CloseValue = ``200 days``.Close
                       MaPrev200AdjCloseValue = ``200 days``.AdjustedClose
                       MaPrev200VolumeValue = ``200 days``.Volume
                       MaPrev252OpenValue = ``252 days``.Open
                       MaPrev252HighValue = ``252 days``.High
                       MaPrev252LowValue = ``252 days``.Low
                       MaPrev252CloseValue = ``252 days``.Close
                       MaPrev252AdjCloseValue = ``252 days``.AdjustedClose
                       MaPrev252VolumeValue = ``252 days``.Volume }
                    : Parameters.NewDayValue)
                    |> Operations.insertDayValue t)))


    ()
