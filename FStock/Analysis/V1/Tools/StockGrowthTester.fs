namespace FStock.Analysis.V1.Tools

open System.IO
open Freql.Sqlite

[<RequireQualifiedAccess>]
module StockGrowthTester =

    open System
    open FStock.Data
    open FStock.Data.Domain

    type Parameters =
        { ResultsStorePath: string
          StartDate: DateTime
          BuyOHLCValue: OHLCValue
          SellOHLCValue: OHLCValue
          TakeProfit: decimal option
          StopLoss: decimal option
          MaxPeriods: int }


    //let getOHLC

    let tableNamePrefix = "stock_result"

    let createTableName (parameters: Parameters) =
        let ohlcPart (ohlcValue: OHLCValue) =
            match ohlcValue with
            | OHLCValue.Open -> "o"
            | OHLCValue.High -> "h"
            | OHLCValue.Low -> "l"
            | OHLCValue.Close -> "c"
            | OHLCValue.AdjustedClose -> "ac"
            | OHLCValue.Volume -> "v"

        let takeProfitPart =
            match parameters.TakeProfit with
            | Some tp -> $"tp_{tp}"
            | None -> "tp_no"

        let stopLossPart =
            match parameters.StopLoss with
            | Some sl -> $"sl_{sl}"
            | None -> "sl_no"

        $"{tableNamePrefix}_{parameters.StartDate:ddMMyy}_{takeProfitPart}_{stopLossPart}_mp_{parameters.MaxPeriods}_b_{ohlcPart parameters.BuyOHLCValue}_s_{ohlcPart parameters.SellOHLCValue}"

    [<CLIMutable>]
    type StockGrowthTestItem =
        { Symbol: string
          BuyPrice: decimal
          SellPrice: decimal option
          Difference: decimal option
          DifferencePercent: decimal option
          DayLength: int option
          ConditionName: string option
          ConditionMessage: string option }

    type TableListingItem =
        { Name: string
          StartDate: DateTime
          BuyValue: string
          SellValue: string
          TakeProfit: decimal option
          StopLoss: decimal option
          MaxPeriods: int }

    let run (store: Store.FStockStore) (parameters: Parameters) =

        use resultsDb =
            match File.Exists parameters.ResultsStorePath with
            | true -> SqliteContext.Open parameters.ResultsStorePath
            | false ->
                use ctx = SqliteContext.Create parameters.ResultsStorePath
                ctx.CreateTable<TableListingItem>("__table_listings") |> ignore

                ctx

        let tableName = createTableName parameters

        resultsDb.CreateTable<StockGrowthTestItem>(tableName) |> ignore

        ({ Name = tableName
           StartDate = parameters.StartDate
           BuyValue = parameters.BuyOHLCValue.Serialize()
           SellValue = parameters.SellOHLCValue.Serialize()
           TakeProfit = parameters.TakeProfit
           StopLoss = parameters.StopLoss
           MaxPeriods = parameters.MaxPeriods }
        : TableListingItem)
        |> fun tli -> resultsDb.Insert<TableListingItem>("__table_listings", tli)

        printfn $"Created table `{tableName}`"
        
        let fetch (symbol: string) =
            store.GetNextXStockEntries(symbol, parameters.StartDate, parameters.MaxPeriods)
            |> List.map (fun s ->
                ({ Date = s.EntryDate
                   Value = s.GetOHLCValue parameters.SellOHLCValue }
                : GrowthTester.FetchedGroupItem))

        let gtp =
            ({ StartDate = parameters.StartDate.AddDays(1)
               FetchHandler = GrowthTester.FetchType.Group fetch
               Condition =
                 GrowthTester.Condition.Any
                     [ match parameters.TakeProfit with
                       | Some tp -> GrowthTester.Condition.TakeProfit tp
                       | None -> ()

                       match parameters.StopLoss with
                       | Some sl -> GrowthTester.Condition.LossStop sl
                       | None -> ()

                       GrowthTester.Condition.FixedPeriod parameters.MaxPeriods ] }
            : GrowthTester.Parameters)

        let run = GrowthTester.run gtp

        store.GetAllStockMetaData()
        |> List.iteri (fun i md ->
            match store.GetStockForDate(md.Symbol, parameters.StartDate) with
            | Some sd ->
                let r = run sd.Symbol sd.CloseValue

                match r with
                | GrowthTester.Finished testResultReport ->
                    let diff = testResultReport.SellPrice - testResultReport.BuyPrice

                    let percentDiff = ((diff) / testResultReport.BuyPrice) * 100m

                    ({ Symbol = testResultReport.Symbol
                       BuyPrice = testResultReport.BuyPrice
                       SellPrice = Some testResultReport.SellPrice
                       Difference = Some diff
                       DifferencePercent = Some percentDiff
                       DayLength = Some <| (testResultReport.EndDate - testResultReport.StartDate).Days
                       ConditionName = Some testResultReport.ConditionName
                       ConditionMessage = Some testResultReport.ConditionMessage }
                    : StockGrowthTestItem)
                    |> fun i -> resultsDb.Insert(tableName, i)
                | GrowthTester.NoResult symbol ->
                    ({ Symbol = symbol
                       BuyPrice = sd.CloseValue
                       SellPrice = None
                       Difference = None
                       DifferencePercent = None
                       DayLength = None
                       ConditionName = None
                       ConditionMessage = None }
                    : StockGrowthTestItem)
                    |> fun i -> resultsDb.Insert(tableName, i)

                printfn $"({i + 1}). {r.Serialize()}"
            | None -> printfn $"({i + 1}). {md.Symbol} - [no data]")
