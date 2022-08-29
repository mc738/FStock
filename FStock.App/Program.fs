open System
open FSVG.Dsl
open FStock.Data
open FStock.Data.Persistence
open FStock.Visualizations.Charts
open Freql.Sqlite


let exampleQuery =
    "SELECT * FROM stocks s WHERE s.symbol = 'TSLA' AND Date(entry_date) >= DATE('2020-01-01') AND DATE(entry_date) < DATE('2020-02-01')"

let total = 24150254

let chunks =
    List.init 2500 (fun i -> i * 10000)
    |> List.map (fun v -> v, 10000)

let ctx =
    SqliteContext.Open "E:\\data\\stock_market\\fstock_store.db"

type Bucket =
    { Min: decimal
      Max: decimal
      Count: int }

let group (points: decimal list) =
    let max = (points |> List.max) + 1m
    let min = points |> List.min

    let diff = max - min

    let size = diff / 10m

    let buckets =
        List.init 10 (fun i -> ((decimal i * size) + min, (decimal (i + 1) * size) + min))
        |> List.map (fun (mi, ma) ->
            { Min = mi
              Max = ma
              Count =
                points
                |> List.filter (fun v -> v >= mi && v < ma)
                |> List.length })


    let total = buckets |> List.sumBy (fun b -> b.Count)
        
    
    let max = buckets |> List.maxBy (fun b -> b.Count) |> fun b -> b.Count
    
    
    

    ()



let t =
    Store.previousXDays ctx 200 (DateTime(2019, 2, 5)) "CRIS"


t
|> List.map (fun s -> s.CloseValue)
|> DistributionChart.generate
|> saveToFile "C:\\ProjectData\\fstock\\test_distribution_chart.svg" 100 100

//|> group

//let t = [ 1m;2m;3m;3m;9m;10m ]

let avg =
    t |> List.averageBy (fun s -> s.CloseValue)

let var =
    t
    |> List.map (fun s -> Math.Pow(float (s.CloseValue - avg), 2))
    |> List.sumBy (fun s -> s)
    |> fun r -> r / (float t.Length)

let sd = Math.Sqrt(var)

//let avg = test |> List.averageBy (fun s -> s.CloseValue)

//let variance =


let dayReports =
    Store.getDay ctx (DateTime(2019, 2, 5))

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

// Stable -
// 50 day high and low diff -
// 200 day high and low diff -



(*
List.init 2500 (fun i -> i * 10000)
|> List.map (fun v -> v, 10000)
|> List.iter (fun (offset, limit) ->

    Persistence.Operations.selectStockRecords ctx [ "LIMIT @0 OFFSET @1" ] [ limit; offset ]
    |> List.iter (fun sr ->
        ({ Symbol = sr.Symbol
           EntryDate = sr.EntryDate
           MovingAverage50Days =
             Store.previousXDays ctx 50 sr.EntryDate sr.Symbol
             |> List.averageBy (fun s -> s.CloseValue)
           MovingAverage200Days =
             Store.previousXDays ctx 200 sr.EntryDate sr.Symbol
             |> List.averageBy (fun s -> s.CloseValue) }: Persistence.Parameters.NewMovingAverage)
        |> Operations.insertMovingAverage ctx)

    ctx.ExecuteInTransaction (fun t ->
        //let records =
        let records =
            Persistence.Operations.selectStockRecords t [ "LIMIT @0 OFFSET @1" ] [ limit; offset ]

        records
        |> List.iteri (fun i sr ->
            ({ Symbol = sr.Symbol
               EntryDate = sr.EntryDate
               MovingAverage50Days =
                 Store.previousXDays t 50 sr.EntryDate sr.Symbol
                 |> List.averageBy (fun s -> s.CloseValue)
               MovingAverage200Days =
                 Store.previousXDays t 200 sr.EntryDate sr.Symbol
                 |> List.averageBy (fun s -> s.CloseValue) }: Persistence.Parameters.NewMovingAverage)
            |> Operations.insertMovingAverage t

            printfn $"{i}/{records.Length} added.")

        printfn $"{offset} ({limit}) selected")
    |> fun r ->
        match r with
        | Ok _ -> printfn "Added."
        | Error e -> printfn $"Error: {e}")
*)

//let allRecords = Persistence.Operations.selectStockRecords ctx [ "LIMIT 10000 OFFSET 10000" ] []

Persistence.Operations.selectStockRecords
    ctx
    [ "WHERE symbol = @0 AND Date(entry_date) >= DATE(@1) AND DATE(entry_date) < DATE(@2)" ]
    [ "TSLA"
      DateTime(2020, 1, 1)
      DateTime(2020, 2, 1) ]
|> List.map (fun s ->
    ({ Date = s.EntryDate
       Open = s.OpenValue
       Close = s.CloseValue
       High = s.HighValue
       Low = s.LowValue
       AdjClose = s.AdjustedCloseValue
       Volume = s.VolumeValue
       MovingAverage200 = 0m
       MovingAverage50 = 0m }: DataPoint))


(*
[ ({ Date = DateTime(2020, 1, 1)
     Open = 10m
     Close = 10m
     High = 15m
     Low = 10m
     AdjClose = 10m
     Volume = 100m }: DataPoint)
  ({ Date = DateTime(2020, 2, 1)
     Open = 12.5m
     Close = 12.5m
     High = 15m
     Low = 10m
     AdjClose = 10m
     Volume = 100m }: DataPoint)
  ({ Date = DateTime(2020, 3, 1)
     Open = 15m
     Close = 15m
     High = 17.5m
     Low = 12.5m
     AdjClose = 15m
     Volume = 100m }: DataPoint)
  ({ Date = DateTime(2020, 4, 1)
     Open = 17.5m
     Close = 17.5m
     High = 20m
     Low = 15m
     AdjClose = 20m
     Volume = 100m }: DataPoint)
  ({ Date = DateTime(2020, 5, 1)
     Open = 20m
     Close = 20m
     High = 20m
     Low = 17.5m
     AdjClose = 20m
     Volume = 100m }: DataPoint) ]
*)
|> generateCandleStickChart ({ ValuePaddingPercent = 20m })
|> saveToFile "C:\\ProjectData\\fstock\\test_chart.svg" 120 100

//Import.build Import.logToConsole "E:\\data\\stock_market"

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
