open System
open FSVG.Dsl
open FStock.Data
open FStock.Visualizations.Charts
open Freql.Sqlite

let exampleQuery =
    "SELECT * FROM stocks s WHERE s.symbol = 'TSLA' AND Date(entry_date) >= DATE('2020-01-01') AND DATE(entry_date) < DATE('2020-02-01')"

let ctx =
    SqliteContext.Open "E:\\data\\stock_market\\fstock_store.db"

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
       Volume = s.VolumeValue }: DataPoint))


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
|> saveToFile "C:\\ProjectData\\fstock\\test_chart.svg"



//Import.build Import.logToConsole "E:\\data\\stock_market"

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
