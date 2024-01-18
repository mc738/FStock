open System
open System.IO
open System.Text.Json
open FSVG
open FSVG.Charts
open FSVG.Dsl
open FStock.Analysis
open FStock.Analysis.V1
open FStock.Data
open FStock.Data.Persistence
open FStock.Data.Store
open FStock.Simulations.V1
open FStock.Visualizations.Charts
open Freql.Sqlite

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

module Simulation =

    let run _ =

        let storeCtx = SqliteContext.Open "E:\\data\\stock_market\\fstock_store.db"
        let start = DateTime(2019, 01, 02)

        match Store.getStockForDate ctx start "GOOG" with
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
                ({ ValueMode = ValueMode.Close
                   ActionCombinationMode = ActionCombinationMode.Simple }
                : SimulationSettings)

            let conditions =
                PositionCondition.Any [ PositionCondition.PercentageGrowth 50m; PositionCondition.PercentageLoss 50m ]

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
                               AdjustedClose = cs.AdjustedCloseValue }
                            : CurrentPosition)

                        let v =
                            match settings.ValueMode with
                            | ValueMode.Open -> cp.Open
                            | ValueMode.Close -> cp.Close
                            | ValueMode.High -> cp.High
                            | ValueMode.Low -> cp.Low
                            | ValueMode.AdjustedClose -> cp.AdjustedClose

                        if conditions.Test(positionStart, cp, settings) then
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


Simulation.run ()
let r = Analytics.build ()

NewTest.run ctx



// Stable -
// 50 day high and low diff -
// 200 day high and low diff -

//Import.build Import.logToConsole "E:\\data\\stock_market"

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
