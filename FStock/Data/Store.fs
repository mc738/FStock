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
    
    
    module Queries =

        type EntryQuery =
            { From: DateTime option
              FromInclusive: bool
              To: DateTime option
              ToInclusive: bool
              SymbolFilter: SymbolFilter }

            member eq.Build() =
                let (tsSql, ps) =

                    match eq.From, eq.FromInclusive, eq.To, eq.ToInclusive with
                    | None, _, None, _ -> None, []
                    | None, _, Some value, true -> Some "DATE(entry_date) <= DATE(@0)", [ box value ]
                    | None, _, Some value, false -> Some "DATE(entry_date) < DATE(@0)", [ box value ]
                    | Some value, true, None, _ -> Some "DATE(entry_date) >= DATE(@0)", [ box value ]
                    | Some value, true, Some value1, true ->
                        Some "DATE(entry_date) >= DATE(@0) AND DATE(entry_date) <= DATE(@1)", [ box value; box value1 ]
                    | Some value, true, Some value1, false ->
                        Some "DATE(entry_date) >= DATE(@0) AND DATE(entry_date) <= DATE(@1)", [ box value; box value1 ]
                    | Some value, false, None, _ -> Some "DATE(entry_date) > DATE(@0)", [ box value ]
                    | Some value, false, Some value1, true ->
                        Some "DATE(entry_date) > DATE(@0) AND DATE(entry_date) <= DATE(@1)", [ box value; box value1 ]
                    | Some value, false, Some value1, false ->
                        Some "DATE(entry_date) > DATE(@0) AND DATE(entry_date) < DATE(@1)", [ box value; box value1 ]

                let (sfSql, sfp) =
                    match eq.SymbolFilter with
                    | SymbolFilter.All -> None, []
                    | SymbolFilter.Stocks -> failwith "todo"
                    | SymbolFilter.Etfs -> failwith "todo"
                    | SymbolFilter.In symbols ->
                        let parts =
                            symbols |> List.mapi (fun i s -> $"@{i + ps.Length}", box s) |> List.unzip

                        parts
                        |> fst
                        |> String.concat ","
                        |> fun r -> Some $"symbol IN ({r})", parts |> snd
                    | SymbolFilter.NotIn symbols ->
                        let parts =
                            symbols |> List.mapi (fun i s -> $"@{i + ps.Length}", box s) |> List.unzip

                        parts
                        |> fst
                        |> String.concat ","
                        |> fun r -> Some $"symbol NOT IN ({r})", parts |> snd
                    | SymbolFilter.EqualTo symbol -> Some $"symbol = @{ps.Length}", [ symbol ]
                    | SymbolFilter.NotEqualTo symbol -> Some $"symbol <> @{ps.Length}", [ symbol ]

                let sql =
                    // TODO check if query is empty.
                    [ tsSql; sfSql ] |> List.choose id |> String.concat " AND "
                    |> fun r -> $"WHERE {r}"
                
                sql, ps @ sfp

        and [<RequireQualifiedAccess>] SymbolFilter =
            | All
            | Stocks
            | Etfs
            | In of Symbols: string list
            | NotIn of Symbols: string list
            | EqualTo of Symbol: string
            | NotEqualTo of Symbol: string

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

    let getAllMetadata (ctx: SqliteContext) =
        Operations.selectSymbolMetadataItemRecords ctx [] []

    let getAllStockMetadata (ctx: SqliteContext) =
        Operations.selectSymbolMetadataItemRecords ctx [ "WHERE is_eft = 0" ] []

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
        let sql =
            """
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

    let executeStockQuery (ctx: SqliteContext) (query: Queries.EntryQuery) =
        let (sql, p) = query.Build()
        
        Operations.selectStockRecords ctx [ sql ] p
        
    
    type FStockStore(ctx: SqliteContext) =

        interface IDisposable with
            member this.Dispose() = (ctx :> IDisposable).Dispose()

        static member Open(path: string) =
            new FStockStore(SqliteContext.Open path)

        member _.GetStockForDate(symbol: string, date: DateTime) = getStockForDate ctx date symbol

        member _.GetPreviousXStockEntries(symbol: string, date: DateTime, x: int) =
            previousXStockEntries ctx x date symbol
            
        member _.GetPreviousXStockEntriesInclusive(symbol: string, date: DateTime, x: int) =
            previousXStockEntriesInclusive ctx x date symbol

        member _.ExecuteStockQuery(query: Queries.EntryQuery) = executeStockQuery ctx query