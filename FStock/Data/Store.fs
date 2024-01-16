namespace FStock.Data

open System
open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open Freql.Core.Common.Types


module Store =

    open Freql.Sqlite
    open FStock.Data.Persistence

    type DayReport =
        { [<JsonPropertyName("date")>]
          Date: DateTime
          [<JsonPropertyName("items")>]
          Items: DayReportItem seq }

    and DayReportItem =
        { [<JsonPropertyName("symbol")>]
          Symbol: string
          [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("movingAverage50Days")>]
          MovingAverage50Days: decimal
          [<JsonPropertyName("movingAverage200Days")>]
          MovingAverage200Days: decimal
          [<JsonPropertyName("previous50DayHigh")>]
          Previous50DayHigh: decimal
          [<JsonPropertyName("previous50DayHighDate")>]
          Previous50DayHighDate: DateTime
          [<JsonPropertyName("previous50DayLow")>]
          Previous50DayLow: decimal
          [<JsonPropertyName("previous50DayLowDate")>]
          Previous50DayLowDate: DateTime
          [<JsonPropertyName("previous50DayVariance")>]
          Previous50DayVariance: float
          [<JsonPropertyName("previous50DayStandardDeviation")>]
          Previous50DayStandardDeviation: float
          [<JsonPropertyName("previous200DayHigh")>]
          Previous200DayHigh: decimal
          [<JsonPropertyName("previous200DayHighDate")>]
          Previous200DayHighDate: DateTime
          [<JsonPropertyName("previous200DayLow")>]
          Previous200DayLow: decimal
          [<JsonPropertyName("previous200DayLowDate")>]
          Previous200DayLowDate: DateTime
          [<JsonPropertyName("previous200DayVariance")>]
          Previous200DayVariance: float
          [<JsonPropertyName("previous200DayStandardDeviation")>]
          Previous200DayStandardDeviation: float
          Stock: Records.Stock
          PreviousDay: Records.Stock }

    let previousXDays (ctx: SqliteContext) (days: int) (date: DateTime) (symbol: string) =
        Operations.selectStockRecords
            ctx
            [ "WHERE symbol = @0 AND DATE(entry_date) >= DATE(@1) AND DATE(entry_date) < DATE(@1)" ]
            [ symbol; date.AddDays(float (days * -1)); date ]

    let getStockForDate (ctx: SqliteContext) (date: DateTime) (symbol: string) =
        Operations.selectStockRecord ctx [ "WHERE symbol = @0 AND DATE(entry_date) == DATE(@1)" ] [ symbol; date ]

    let getMetadata (ctx: SqliteContext) (symbol: string) =
        Operations.selectSymbolMetadataItemRecord ctx [ "WHERE symbol = @0" ] [ symbol ]

    let previousXStockEntries (ctx: SqliteContext) (x: int) (date: DateTime) (symbol: string) =
        Operations.selectStockRecords
            ctx
            [ "WHERE symbol = @0 AND DATE(entry_date) < DATE(@1)"
              "ORDER BY entry_date DESC"
              "LIMIT @2" ]
            [ symbol; date; x ]
            
    let previousXStockEntriesInclusive (ctx: SqliteContext) (x: int) (date: DateTime) (symbol: string) =
        Operations.selectStockRecords
            ctx
            [ "WHERE symbol = @0 AND DATE(entry_date) <= DATE(@1)"
              "ORDER BY entry_date DESC"
              "LIMIT @2" ]
            [ symbol; date; x ]

    let nextXStockEntries (ctx: SqliteContext) (x: int) (date: DateTime) (symbol: string) =
        Operations.selectStockRecords
            ctx
            [ "WHERE symbol = @0 AND DATE(entry_date) > DATE(@1)"
              "ORDER BY entry_date"
              "LIMIT @2" ]
            [ symbol; date; x ]
            
    let calculateMoveAverageX (ctx: SqliteContext) =
        let sql = """
        SELECT
	        symbol,
	        entry_date,
	        AVG(open_value),
	        AVG(high_value),
	        AVG(low_value),
	        AVG(close_value),
	        AVG(adjusted_close_value),
	        AVG(volume_value),
	        COUNT(symbol)
        FROM
	        (
	        SELECT
		        symbol,
		        entry_date,
		        open_value,
		        high_value,
		        low_value,
		        close_value,
		        adjusted_close_value,
		        volume_value,
		        symbol
	        FROM
		        stocks
	        WHERE
		        symbol = @0
		        AND DATE(entry_date) < DATE(@1)
	        ORDER BY
		        entry_date
	        LIMIT @2)
        """
        
        
        
        ()
        
        
    

    let generateDayReport (ctx: SqliteContext) (date: DateTime) =
        Operations.selectSymbolMetadataItemRecords ctx [] []
        |> List.filter (fun md -> md.IsEft |> not)
        |> List.choose (fun md ->
            match getStockForDate ctx date md.Symbol with
            | Some s ->
                let prev200 = previousXDays ctx 200 date s.Symbol

                let prev50 = previousXDays ctx 50 date s.Symbol

                let prev50Avg = prev50 |> List.averageBy (fun s -> s.CloseValue)

                let prev200Avg = prev200 |> List.averageBy (fun s -> s.CloseValue)

                let prev200High = prev200 |> List.maxBy (fun s -> s.CloseValue)

                let prev200Low = prev200 |> List.minBy (fun s -> s.CloseValue)

                let prev50High = prev50 |> List.maxBy (fun s -> s.CloseValue)

                let prev50Low = prev50 |> List.minBy (fun s -> s.CloseValue)

                let prev50Var =
                    prev50
                    |> List.map (fun s -> Math.Pow(float (s.CloseValue - prev50Avg), 2))
                    |> List.sumBy (fun s -> s)
                    |> fun r -> r / (float prev50.Length)

                let prev200Var =
                    prev200
                    |> List.map (fun s -> Math.Pow(float (s.CloseValue - prev200Avg), 2))
                    |> List.sumBy (fun s -> s)
                    |> fun r -> r / (float prev200.Length)

                ({ Symbol = s.Symbol
                   Name = md.SecurityName
                   MovingAverage50Days = prev50Avg
                   MovingAverage200Days = prev200Avg
                   Previous50DayHigh = prev50High.CloseValue
                   Previous50DayHighDate = prev50High.EntryDate
                   Previous50DayLow = prev50Low.CloseValue
                   Previous50DayLowDate = prev50Low.EntryDate
                   Previous50DayVariance = prev50Var
                   Previous50DayStandardDeviation = Math.Sqrt prev50Var
                   Previous200DayHigh = prev200High.CloseValue
                   Previous200DayHighDate = prev200High.EntryDate
                   Previous200DayLow = prev200Low.CloseValue
                   Previous200DayLowDate = prev200Low.EntryDate
                   Previous200DayVariance = prev200Var
                   Previous200DayStandardDeviation = Math.Sqrt prev200Var
                   Stock = s
                   PreviousDay = prev200 |> List.rev |> List.head }
                : DayReportItem)
                |> Some
            | None -> None)
        |> fun r -> ({ Date = date; Items = r |> Seq.ofList }: DayReport)


    let getDay (ctx: SqliteContext) (date: DateTime) =
        Operations.selectDayReportRecord ctx [ "WHERE DATE(entry_date) = DATE(@0)" ] [ date ]
        |> Option.map (fun dr ->
            printfn "Report found."

            dr.ReportBlob.ToBytes()
            |> Encoding.UTF8.GetString
            |> JsonSerializer.Deserialize<DayReport>)
        |> Option.defaultWith (fun _ ->
            printfn "Report not found. Generating (this could take some time)."
            let report = generateDayReport ctx date
            printfn "Caching report."

            use ms =
                new MemoryStream(report |> JsonSerializer.Serialize |> Encoding.UTF8.GetBytes)

            ({ EntryDate = date
               ReportBlob = BlobField.FromStream ms }
            : Parameters.NewDayReport)
            |> Operations.insertDayReport ctx

            report)
