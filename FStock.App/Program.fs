open System
open System.IO
open System.Text.Json
open FSVG
open FSVG.Charts
open FSVG.Dsl
open FStock.Analysis
open FStock.Analysis.V1
open FStock.Analysis.V1.Store
open FStock.Analysis.V1.Tools
open FStock.App
open FStock.Data
open FStock.Data.Domain
open FStock.Data.Persistence
open FStock.Data.Store
open FStock.Modeling.V1
open FStock.Visualizations.Charts
open Freql.Sqlite
open Microsoft.FSharp.Core

module DistributionChartTest =

    let run (ctx: SqliteContext) (symbol: string) =


        let t = Store.previousXDays ctx 200 (DateTime(2019, 2, 5)) symbol

        t
        |> List.map (fun s -> s.CloseValue)
        |> DistributionChart.generate
        |> saveToFile "C:\\ProjectData\\fstock\\test_distribution_chart.svg" 100 100

module CandleStickChartTest =

    let run (ctx: SqliteContext) (symbol: string) =

        Persistence.Operations.selectStockRecords
            ctx
            [ "WHERE symbol = @0 AND Date(entry_date) >= DATE(@1) AND DATE(entry_date) < DATE(@2)" ]
            [ symbol; DateTime(2020, 1, 1); DateTime(2020, 2, 1) ]
        |> List.map (fun s ->
            ({ Date = s.EntryDate
               Open = s.OpenValue
               Close = s.CloseValue
               High = s.HighValue
               Low = s.LowValue
               AdjClose = s.AdjustedCloseValue
               Volume = s.VolumeValue
               MovingAverage200 = 0m
               MovingAverage50 = 0m }
            : DataPoint))
        |> generateCandleStickChart ({ ValuePaddingPercent = 20m })
        |> saveToFile "C:\\ProjectData\\fstock\\test_chart.svg" 120 100

module TestStrategy =

    let run (ctx: SqliteContext) (startDate: DateTime) (item: DayReportItem) =
        printfn $"Test {item.Symbol}"

        [ 5.; 10.; 20.; 30.; 50.; 100.; 200.; 365. ]
        |> List.iter (fun v ->
            match Store.getStockForDate ctx (startDate.AddDays(v)) item.Symbol with
            | Some s -> printfn $"\tValue after {v} days: {s.CloseValue} ({s.CloseValue - item.Stock.CloseValue})"
            | None -> printfn $"\tNo data for +{v} days.")

(*
module NewTest =

    let run (ctx: SqliteContext) =

        let test = History.build ctx "GOOG" (DateTime(2019, 01, 02)) 100

        let jsOpt = JsonSerializerOptions()

        jsOpt.WriteIndented <- true

        JsonSerializer.Serialize(test, jsOpt)
        |> fun b -> File.WriteAllText("E:\\data\\stock_market\\exports\\example.json", b)


        let settings =
            ({ ChartDimensions =
                { Height = 100.
                  Width = 100.
                  LeftOffset = 10
                  BottomOffset = 10
                  TopOffset = 10
                  RightOffset = 10 }
               Title = Some "Google Comparison"
               XLabel = Some "Value"
               YLabel = Some "Date"
               LegendStyle =
                 ({ Bordered = false
                    Position = LegendPosition.Right }
                 : LegendStyle)
                 |> Some
               YMajorMarkers = [ 50; 100 ]
               YMinorMarkers = [ 25; 75 ] }
            : LineCharts.Settings)

        (*
        "Item 1"
        "Item 2"
        "Item 3"
        "Item 4"
        "Item 5"
        *)
        let items = test.Items |> List.ofSeq |> List.rev

        let seriesCollection =
            ({ SplitValueHandler = valueSplitter float
               Normalizer = rangeNormalizer<decimal> float
               PointNames = items |> List.map (fun i -> i.EntryDate.ToString("dd'-'MM")) //[ "Item 1"; "Item 2"; "Item 3"; "Item 4"; "Item 5" ]
               Series =
                 [ ({ Name = "7 day moving average"
                      Style =
                        { Color = SvgColor.Grey
                          StrokeWidth = 0.3
                          LineType = LineCharts.LineType.Straight
                          Shading =
                            ({ Color = SvgColor.Rgba(255uy, 0uy, 0uy, 0.3) }: LineCharts.ShadingOptions)
                            |> Some }
                      Values = items |> List.map (fun i -> i.Previous7MovingAverage.Close) }
                   : LineCharts.Series<decimal>)
                   ({ Name = "20 day moving average"
                      Style =
                        { Color = SvgColor.Black
                          StrokeWidth = 0.3
                          LineType = LineCharts.LineType.Bezier
                          Shading = None }
                      Values = items |> List.map (fun i -> i.Previous20MovingAverage.Close) }
                   : LineCharts.Series<decimal>)
                   ({ Name = "Close value"
                      Style =
                        { Color = SvgColor.Named "green"
                          StrokeWidth = 0.3
                          LineType = LineCharts.LineType.Bezier
                          Shading = None }
                      Values = items |> List.map (fun i -> i.Close) }
                   : LineCharts.Series<decimal>) ] }
            : LineCharts.SeriesCollection<decimal>)

        let min = (items |> List.minBy (fun i -> i.Low)) |> fun r -> r.Low
        let max = (items |> List.maxBy (fun i -> i.High)) |> fun r -> r.High

        LineCharts.generate settings seriesCollection min max
        |> fun r -> File.WriteAllText("E:\\data\\stock_market\\exports\\example_chart.svg", r)



        ()
*)        
        

let ctx = SqliteContext.Open "E:\\data\\stock_market\\fstock_store.db"

let old _ =

    let dayReports = Store.getDay ctx (DateTime(2019, 2, 5))

    let interesting =
        dayReports.Items

        |> Seq.filter (fun dri ->
            dri.MovingAverage50Days < dri.MovingAverage200Days
            && dri.Stock.OpenValue > dri.PreviousDay.CloseValue
            && dri.Previous50DayStandardDeviation > (float (dri.MovingAverage50Days / 100m) * 10.)
            && dri.Previous200DayStandardDeviation > (float (dri.MovingAverage200Days / 100m) * 10.)
            && dri.Previous50DayLow > dri.Previous200DayLow
            && dri.Stock.OpenValue < ((dri.MovingAverage200Days / 100m) * 90m)
        //&& dri.Previous50DayHigh > dri.Previous200DayHigh
        )
        |> List.ofSeq

    interesting |> List.iter (TestStrategy.run ctx dayReports.Date)

(*
module Analytics =

    open FStock.Analysis.V1

    let build _ =

        let storeCtx = SqliteContext.Open "E:\\data\\stock_market\\fstock_store.db"

        ({ SampleSize = 1.
           Date = (DateTime(2019, 01, 02))
           LookBackCount = 30
           Path = "C:\\ProjectData\\fstock\\analytics_test.db" }
        : BuildParameters)
        |> build storeCtx

    ()
*)

module Backtester =

    open FStock.App.Backtester.V1

    let initialState (startDate: DateTime) =
        ({ Portfolio =
            { Capital = 1000m
              Liquidity = 0m
              OpenPositions = []
              ClosedPositions = failwith "todo" }
           Model =
             { Behaviours = Map.empty
               GeneralBehaviours = failwith "todo"
               DefaultBehaviour = failwith "todo" }
           StartDate = startDate
           CurrentDate = startDate
           BehaviourMaps =
             [ { BehaviourId = failwith "todo"
                 PositionId = ""
                 Priority = 1 } ] }
        : FStock.Modeling.V1.Backtesting.ModelState)

    let start _ =

        let storePath = "E:\\data\\stock_market"

        let start = DateTime(2019, 1, 2)

        let state = initialState start

        let onProgression (state: Backtesting.ModelState) (log: string list) = true

        let finalState = Backtester.V1.run storePath start state onProgression

        ()

module Tests =

    open FStock.Analysis.V1.TechnicalIndicators
    open FStock.Analysis.V1.Core

    (*
    let rsi _ =
        let values: BasicInputItem list =
            [ { Date = DateTime(2024, 1, 1)
                Price = 54.8m }
              { Date = DateTime(2024, 1, 2)
                Price = 56.8m }
              { Date = DateTime(2024, 1, 3)
                Price = 57.85m }
              { Date = DateTime(2024, 1, 4)
                Price = 59.85m }
              { Date = DateTime(2024, 1, 5)
                Price = 60.57m }
              { Date = DateTime(2024, 1, 6)
                Price = 61.1m }
              { Date = DateTime(2024, 1, 7)
                Price = 62.17m }
              { Date = DateTime(2024, 1, 8)
                Price = 60.6m }
              { Date = DateTime(2024, 1, 9)
                Price = 62.35m }
              { Date = DateTime(2024, 1, 10)
                Price = 62.15m }
              { Date = DateTime(2024, 1, 11)
                Price = 62.35m }
              { Date = DateTime(2024, 1, 12)
                Price = 61.45m }
              { Date = DateTime(2024, 1, 13)
                Price = 62.8m }
              { Date = DateTime(2024, 1, 14)
                Price = 61.37m }
              { Date = DateTime(2024, 1, 15)
                Price = 62.5m }
              { Date = DateTime(2024, 1, 16)
                Price = 62.57m }
              { Date = DateTime(2024, 1, 17)
                Price = 60.8m }
              { Date = DateTime(2024, 1, 18)
                Price = 59.37m }
              { Date = DateTime(2024, 1, 19)
                Price = 60.35m }
              { Date = DateTime(2024, 1, 20)
                Price = 62.35m }
              { Date = DateTime(2024, 1, 21)
                Price = 62.17m }
              { Date = DateTime(2024, 1, 22)
                Price = 62.55m }
              { Date = DateTime(2024, 1, 23)
                Price = 64.55m }
              { Date = DateTime(2024, 1, 24)
                Price = 64.37m }
              { Date = DateTime(2024, 1, 25)
                Price = 65.3m }
              { Date = DateTime(2024, 1, 26)
                Price = 64.42m }
              { Date = DateTime(2024, 1, 27)
                Price = 62.9m }
              { Date = DateTime(2024, 1, 28)
                Price = 61.6m }
              { Date = DateTime(2024, 1, 29)
                Price = 62.05m }
              { Date = DateTime(2024, 1, 30)
                Price = 60.05m }
              { Date = DateTime(2024, 1, 31)
                Price = 59.7m }
              { Date = DateTime(2024, 2, 1)
                Price = 60.9m }
              { Date = DateTime(2024, 2, 2)
                Price = 60.25m }
              { Date = DateTime(2024, 2, 3)
                Price = 58.27m }
              { Date = DateTime(2024, 2, 4)
                Price = 58.7m }
              { Date = DateTime(2024, 2, 5)
                Price = 57.72m }
              { Date = DateTime(2024, 2, 6)
                Price = 58.1m }
              { Date = DateTime(2024, 2, 7)
                Price = 58.2m } ]

        let parameters =
            ({ Periods = 14
               RoundHandler = fun v -> Math.Round(v, 2, MidpointRounding.AwayFromZero) }
            : RelativeStrengthIndex.Parameters)

        let result = RelativeStrengthIndex.calculate parameters values

        ()

    let ema _ =
        let values: BasicInputItem list =
            [ { Date = DateTime(2024, 1, 1)
                Price = 42.42m }
              { Date = DateTime(2024, 1, 2)
                Price = 43.27m }
              { Date = DateTime(2024, 1, 3)
                Price = 43.66m }
              { Date = DateTime(2024, 1, 4)
                Price = 43.4m }
              { Date = DateTime(2024, 1, 5)
                Price = 43.4m }
              { Date = DateTime(2024, 1, 6)
                Price = 44.27m }
              { Date = DateTime(2024, 1, 7)
                Price = 45.01m }
              { Date = DateTime(2024, 1, 8)
                Price = 44.48m }
              { Date = DateTime(2024, 1, 9)
                Price = 44.34m }
              { Date = DateTime(2024, 1, 10)
                Price = 44.44m }
              { Date = DateTime(2024, 1, 11)
                Price = 44.08m }
              { Date = DateTime(2024, 1, 12)
                Price = 44.16m }
              { Date = DateTime(2024, 1, 13)
                Price = 44.04m }
              { Date = DateTime(2024, 1, 14)
                Price = 43.74m }
              { Date = DateTime(2024, 1, 15)
                Price = 44.27m }
              { Date = DateTime(2024, 1, 16)
                Price = 44.11m }
              { Date = DateTime(2024, 1, 17)
                Price = 43.93m }
              { Date = DateTime(2024, 1, 18)
                Price = 44.35m }
              { Date = DateTime(2024, 1, 19)
                Price = 45.21m }
              { Date = DateTime(2024, 1, 20)
                Price = 44.92m } ]

        let parameters =
            ({ Smoothing = 2m; WindowSize = 12 }: ExponentialMovingAverage.Parameters)

        let result = ExponentialMovingAverage.calculate parameters values

        ()
*)
    (*
    module DeleteMe =

        type Part<'T> =
            | And of Part<'T> * Part<'T>
            | Or of Part<'T> * Part<'T>
            | Part of 'T


            static member (?&&)(a: Part<'T>, b: Part<'T>) = And(a, b)

        type StringQueryPart = { TableName: string }

        type DateTimeQueryPart = { TableName: string }

        [<RequireQualifiedAccess>]
        type StocksQueryPart =
            | Foo of StringQueryPart
            | Bar of DateTimeQueryPart

            member sqp.Build() = "", []

        let buildQuery (parts: Part<StocksQueryPart>) =
            let rec build (i, p) = ()

            //parts |> List.map (fun i -> i)



            ()

        let (!&&) a b = Part a, Part b

        let t =
            Part(StocksQueryPart.Foo { TableName = "" })
            ?&& Part(StocksQueryPart.Bar { TableName = "" })

        let i =
            !&& (StocksQueryPart.Foo { TableName = "" }) (StocksQueryPart.Bar { TableName = "" })

        And((StocksQueryPart.Foo { TableName = "" }) !&&(StocksQueryPart.Bar { TableName = "" }))
        |> buildQuery


        ()
    *)

    let chart _ =
        let storePath = "E:\\data\\stock_market\\fstock_store.db"

        let start = DateTime(2019, 6, 2)

        use store = FStockStore.Open storePath

        let firstQuery =
            ({ From = start.AddMonths(-3) |> Some
               FromInclusive = true
               To = Some start
               ToInclusive = true
               SymbolFilter = Queries.SymbolFilter.EqualTo "AFI" }
            : Queries.EntryQuery)

        let baseData = store.ExecuteStockQuery(firstQuery)

        let auxData =
            store.GetPreviousXStockEntries(
                "AFI",
                baseData |> List.minBy (fun e -> e.EntryDate) |> (fun e -> e.EntryDate),
                200
            )

        Visualizations.Predefined.generate
            { BaseData = baseData
              AuxData = auxData }
        |> fun r -> File.WriteAllText("C:\\ProjectData\\fstock\\test_chart_new.svg", r)

    let buildStore _ =
        let storePath = "E:\\data\\stock_market\\fstock_store.db"

        let path = "C:\\ProjectData\\fstock\\investigation_06_01_2015\\analytics_store_06_01_2015.db"

        let settings =
            ({ Sampling = SamplingType.None
               BuildMode = BuildMode.Overwrite
               TechnicalIndicators =
                 [ TechnicalIndicator.Sma("sma_50", { WindowSize = 50 })
                   TechnicalIndicator.Sma("sma_200", { WindowSize = 200 })
                   TechnicalIndicator.Macd("macd", MovingAverageConvergenceDivergence.Parameters.Default())
                   TechnicalIndicator.Rsi("rsi", RelativeStrengthIndex.Parameters.Default()) ] }
            : BuildSettings)

        let start = DateTime(2015, 1, 6)
        
        Store.build settings (FStock.Data.Store.FStockStore.Open storePath) path start

    [<CLIMutable>]
    type SqliteGrowthTestItem =
        {
            Symbol: string
            BuyPrice: decimal
            SellPrice: decimal option
            Difference: decimal option
            DifferencePercent: decimal option
            DayLength: int option
            ConditionName: string option
            ConditionMessage: string option
        }
    
    let growthTester _ =
        let storePath = "E:\\data\\stock_market\\fstock_store.db"
        
        let store = FStockStore.Open storePath
        let startDate = DateTime(2018, 01, 2)
        
        let resultsDb = SqliteContext.Open("C:\\ProjectData\\fstock\\growth_test_results.db")
        
        let tableName = "results_020118_tk_10_sl_no"
        
        resultsDb.CreateTable<SqliteGrowthTestItem>(tableName) |> ignore
        
        let parameters =
            
            let fetch1 (symbol: string) (date: DateTime) =
                store.GetStockForDate(symbol, date) |> Option.map (fun s -> s.CloseValue)
            
            let fetch2 (symbol: string) =
                store.GetNextXStockEntries(symbol, startDate, 365)
                |> List.map (fun s -> ({ Date = s.EntryDate; Value = s.CloseValue }: GrowthTester.FetchedGroupItem))
                 
            let dbFetch = GrowthTester.FetchType.Individual fetch1
            
            let groupFetch = GrowthTester.FetchType.Group fetch2
                
            ({ StartDate = startDate.AddDays(1)
               FetchHandler = groupFetch
               Condition =
                 GrowthTester.Condition.Any
                     [ GrowthTester.Condition.TakeProfit 10m
                       //GrowthTester.Condition.LossStop 20m
                       GrowthTester.Condition.FixedPeriod 365 ] }
            : GrowthTester.Parameters)


        let run = GrowthTester.run parameters
        
        store.GetAllStockMetaData()
        |> List.iteri (fun i md ->
            match store.GetStockForDate(md.Symbol, startDate) with
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
                       ConditionMessage = Some testResultReport.ConditionMessage }: SqliteGrowthTestItem)
                    |> fun i -> resultsDb.Insert(tableName, i)
                | GrowthTester.NoResult symbol ->
                    ({ Symbol = symbol
                       BuyPrice = sd.CloseValue
                       SellPrice = None
                       Difference = None
                       DifferencePercent = None
                       DayLength = None
                       ConditionName = None
                       ConditionMessage = None }: SqliteGrowthTestItem)
                    |> fun i -> resultsDb.Insert(tableName, i)
                    
                printfn $"({i + 1}). {r.Serialize()}"
            | None ->
                printfn $"({i + 1}). {md.Symbol} - [no data]")

        ()

    let stockGrowthTester _ =
        
        let storePath = "E:\\data\\stock_market\\fstock_store.db"
        
        let store = FStockStore.Open storePath
        
        let parameters =
            ({
            ResultsStorePath = "C:\\ProjectData\\fstock\\stock_growth_testing_results.db"
            StartDate = DateTime(2015, 01, 06)
            BuyOHLCValue = OHLCValue.Open
            SellOHLCValue = OHLCValue.High
            TakeProfit = Some 10m
            StopLoss = None
            MaxPeriods = 365 
        }: StockGrowthTester.Parameters)
        
        StockGrowthTester.run store parameters
        
        

module Simulation =

    let run _ =

        let storeCtx = SqliteContext.Open "E:\\data\\stock_market\\fstock_store.db"
        let start = DateTime(2019, 05, 31)

        match Store.getStockForDate ctx start "VXRT" with
        | Some s ->
            // For now choose open value.

            let positionStart =
                ({ Id = System.Guid.NewGuid().ToString()
                   ParentId = None
                   Symbol = s.Symbol
                   Start = start
                   BuyPrice = s.OpenValue
                   Volume = 1m }
                : OpenPosition)

            let settings =
                ({ OHLCValue = OHLCValue.Close
                   ActionCombinationMode = ActionCombinationMode.Simple }
                : SimulationSettings)

            let conditions =
                PositionCondition.Any
                    [ PositionCondition.PercentageGrowth(50m, ConditionValueMapper.Value OHLCValue.Close)
                      PositionCondition.PercentageLoss(50m, ConditionValueMapper.Value OHLCValue.Close) ]

            let rec handler (date: DateTime) =
                if (date.Date < DateTime.UtcNow.Date) then
                    match Store.getStockForDate storeCtx date positionStart.Symbol with
                    | Some cs ->
                        let cp =
                            ({ Symbol = cs.Symbol
                               Date = cs.EntryDate
                               Open = cs.OpenValue
                               High = cs.HighValue
                               Low = cs.CloseValue
                               Close = cs.CloseValue
                               AdjustedClose = cs.AdjustedCloseValue
                               Volume = cs.VolumeValue }
                            : CurrentPosition)

                        let v =
                            match settings.OHLCValue with
                            | OHLCValue.Open -> cp.Open
                            | OHLCValue.Close -> cp.Close
                            | OHLCValue.High -> cp.High
                            | OHLCValue.Low -> cp.Low
                            | OHLCValue.AdjustedClose -> cp.AdjustedClose
                            | OHLCValue.Volume -> cp.Volume

                        let profitTakeTest _ =
                            ((positionStart.BuyPrice / 100m) * 5m) + positionStart.BuyPrice < cp.Close

                        let stopLossTest _ =
                            positionStart.BuyPrice - ((positionStart.BuyPrice / 100m) * 20m) > cp.Close

                        if profitTakeTest () || stopLossTest () then
                            let diff = v - positionStart.BuyPrice

                            let percentDiff = ((v - positionStart.BuyPrice) / positionStart.BuyPrice) * 100m

                            //
                            printfn
                                $"Stock `{cs.Symbol}` sold after {(cs.EntryDate - positionStart.Start).Days} day(s)."

                            printfn $"Buy price: {positionStart.BuyPrice}"
                            printfn $"Sell price: {v}"
                            printfn $"Difference: {diff} (%0.2f{percentDiff}%%)"
                            printfn $"Volume: {positionStart.Volume}"
                            printfn $"Profit: {diff * positionStart.Volume}"
                        else
                            handler (date.AddDays(1))
                    | None -> handler (date.AddDays(1))
                else
                    printfn "Not sold, still holding."

            handler (start.AddDays(1))
        | None ->

            printfn "Not found."

        ()



//let _ = Tests.buildStore ()

let _ = Tests.stockGrowthTester ()

let _ = Tests.growthTester ()

let _ = Simulation.run ()

let _ = Tests.chart ()

//Simulation.run ()
//let r11 = Tests.ema ()

//let r1 = Tests.rsi ()
//let r = Analytics.build ()

//NewTest.run ctx



// Stable -
// 50 day high and low diff -
// 200 day high and low diff -

//Import.build Import.logToConsole "E:\\data\\stock_market"

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
