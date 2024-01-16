namespace FStock.Analysis

open FStock.Data
open Freql.Sqlite

[<RequireQualifiedAccess>]
module History =

    open System
    open System.Text.Json.Serialization


    [<RequireQualifiedAccess>]
    module V1 =

        [<CLIMutable>]
        type StockHistory =
            { [<JsonPropertyName("symbol")>]
              Symbol: string
              [<JsonPropertyName("date")>]
              Date: DateTime
              [<JsonPropertyName("length")>]
              Length: int
              [<JsonPropertyName("items")>]
              Items: StockHistoryItem seq }

        and [<CLIMutable>] StockHistoryItem =
            { [<JsonPropertyName("entryDate")>]
              EntryDate: DateTime
              [<JsonPropertyName("previous7MovingAverage")>]
              Previous7MovingAverage: MovingAverageValues
              [<JsonPropertyName("previous20MovingAverage")>]
              Previous20MovingAverage: MovingAverageValues
              [<JsonPropertyName("previous50MovingAverage")>]
              Previous50MovingAverage: MovingAverageValues
              [<JsonPropertyName("previous100MovingAverage")>]
              Previous100MovingAverage: MovingAverageValues
              [<JsonPropertyName("previous200MovingAverage")>]
              Previous200MovingAverage: MovingAverageValues
              [<JsonPropertyName("previousYearMovingAverage")>]
              PreviousYearMovingAverage: MovingAverageValues
              [<JsonPropertyName("open")>]
              Open: decimal
              [<JsonPropertyName("high")>]
              High: decimal
              [<JsonPropertyName("low")>]
              Low: decimal
              [<JsonPropertyName("close")>]
              Close: decimal
              [<JsonPropertyName("adjustedClose")>]
              AdjustedClose: decimal
              [<JsonPropertyName("volume")>]
              Volume: decimal }

        and [<CLIMutable>] MovingAverageValues =
            { [<JsonPropertyName("open")>]
              Open: decimal
              [<JsonPropertyName("high")>]
              High: decimal
              [<JsonPropertyName("low")>]
              Low: decimal
              [<JsonPropertyName("close")>]
              Close: decimal
              [<JsonPropertyName("adjustedClose")>]
              AdjustedClose: decimal
              [<JsonPropertyName("volume")>]
              Volume: decimal
              [<JsonPropertyName("entryCount")>]
              EntryCount: int }

        let getMovingAverages (ctx: SqliteContext) (symbol: string) (date: DateTime) (length: int) =
            let sql =
                """
            SELECT
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
	    	        entry_date DESC
	            LIMIT @2)
            """

            ctx.Bespoke(
                sql,
                [ symbol; date; length ],
                (fun reader ->
                    [ while reader.Read() do
                          yield
                              ({ Open = reader.GetDecimal 0
                                 High = reader.GetDecimal 1
                                 Low = reader.GetDecimal 2
                                 Close = reader.GetDecimal 3
                                 AdjustedClose = reader.GetDecimal 4
                                 Volume = reader.GetDecimal 5
                                 EntryCount = reader.GetInt32 6 }
                              : MovingAverageValues) ])
            )


        let build (ctx: SqliteContext) (symbol: string) (startDate: DateTime) (length: int) =
            //

            ({ Symbol = symbol
               Date = startDate
               Length = length
               Items =
                 Store.previousXStockEntriesInclusive ctx length startDate symbol
                 |> List.map (fun s ->
                     ({ EntryDate = s.EntryDate
                        Previous7MovingAverage = getMovingAverages ctx symbol s.EntryDate 7 |> List.head
                        Previous20MovingAverage = getMovingAverages ctx symbol s.EntryDate 20 |> List.head
                        Previous50MovingAverage = getMovingAverages ctx symbol s.EntryDate 50 |> List.head
                        Previous100MovingAverage = getMovingAverages ctx symbol s.EntryDate 100 |> List.head
                        Previous200MovingAverage = getMovingAverages ctx symbol s.EntryDate 200 |> List.head
                        PreviousYearMovingAverage = getMovingAverages ctx symbol s.EntryDate 252 |> List.head
                        Open = s.OpenValue
                        High = s.HighValue
                        Low = s.LowValue
                        Close = s.CloseValue
                        AdjustedClose = s.AdjustedCloseValue
                        Volume = s.VolumeValue }
                     : StockHistoryItem)) }
            : StockHistory)




        ()
