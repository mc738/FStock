open System
open FSVG.Dsl
open FStock.Data
open FStock.Visualizations.Charts
open Freql.Sqlite

type Stock =
    { Symbol: string
      BroughtOn: DateTime
      BuyPrice: decimal
      Amount: decimal }

type State =
    { Date: DateTime
      Stocks: Stock list
      Credit: decimal }

type StockReport =
    { Symbol: string
      Name: string
      Date: DateTime
      PreviousDate: DateTime
      PreviousClose: decimal
      Previous200Average: decimal
      Previous50Average: decimal
      Open: decimal }

let getReport (ctx: SqliteContext) (date: DateTime) (symbol: string) =
    let prev200 =
        Store.previousXDays ctx 200 date symbol

    let prev50 =
        Store.previousXDays ctx 50 date symbol

    let prev200Avg =
        prev200 |> List.averageBy (fun s -> s.CloseValue)

    let prev50Avg =
        prev50 |> List.averageBy (fun s -> s.CloseValue)

    let prev = prev200 |> List.rev |> List.head

    Store.getStockForDate ctx date symbol
    |> Option.map (fun today ->
        ({ Symbol = today.Symbol
           Name =
             Store.getMetadata ctx today.Symbol
             |> Option.map (fun md -> md.SecurityName)
             |> Option.defaultValue "[unknown]"
           Date = date
           PreviousDate = prev.EntryDate
           PreviousClose = prev.CloseValue
           Previous200Average = prev200Avg
           Previous50Average = prev50Avg
           Open = today.OpenValue }: StockReport))

let printReport ctx (report: StockReport) =
    let prevDay =
        getReport ctx report.PreviousDate report.Symbol


    let diff =
        report.Open - report.PreviousClose

    printfn $"{report.Name} ({report.Symbol})"
    printf $"\tPrevious close: {report.PreviousClose}"

    match prevDay with
    | Some prevReport -> printf $" ({(report.PreviousClose - prevReport.PreviousClose)})"
    | None -> ()

    printfn ""

    printf $"\t200 day moving average: {report.Previous200Average}"

    match prevDay with
    | Some prevReport ->
        printf
            $" ({(report.Previous200Average
                  - prevReport.Previous200Average)})"
    | None -> ()

    printfn ""

    printf $"\t50 day moving average: {report.Previous50Average}"

    match prevDay with
    | Some prevReport ->
        printf
            $" ({(report.Previous50Average
                  - prevReport.Previous50Average)})"
    | None -> ()

    printfn ""

    printf $"\tOpen: {report.Open}"

    match diff > 0m with
    | true -> Console.ForegroundColor <- ConsoleColor.Green
    | false -> Console.ForegroundColor <- ConsoleColor.Red

    printf $" ({diff})"

    Console.ResetColor()
    printfn ""

//let prev200Avg = prev200 |> List.sumBy (fun s -> s.CloseValue)
//let prev50Avg = prev50 |> List.sumBy (fun s -> s.CloseValue)


// For more information see https://aka.ms/fsharp-console-apps

module Simulator =

    let rec run (ctx: SqliteContext, state: State) =
        printf "> "
        let command = Console.ReadLine()

        match command with
        | "report" ->
            printf "Symbol > "
            let symbol = Console.ReadLine()

            match getReport ctx state.Date symbol with
            | Some report -> printReport ctx report
            | None -> printfn $"No report for symbol `{symbol}` on date `{state.Date}`."

            run (ctx, state)
        | "buy" ->
            printf "Symbol > "
            let symbol = Console.ReadLine()
            printf "Amount > "

            let amount =
                Console.ReadLine()
                |> Decimal.TryParse
                |> fun r ->
                    match r with
                    | true, v -> v
                    | false, _ -> 0m

            match Store.getStockForDate ctx state.Date symbol with
            | Some stock ->
                // Buy of open price for now...
                let remainingCredit =
                    state.Credit - (stock.OpenValue * amount)

                match remainingCredit >= 0m with
                | true ->
                    printfn
                        $"{amount} stock brought at {stock.OpenValue} (total: {stock.OpenValue * amount}). Remaining credit: {remainingCredit}"

                    let newState =
                        { state with
                            Credit = remainingCredit
                            Stocks =
                                state.Stocks
                                @ [ ({ Symbol = symbol
                                       BroughtOn = state.Date
                                       BuyPrice = stock.OpenValue
                                       Amount = amount }: Stock) ] }

                    run (ctx, newState)
                | false ->
                    printfn "You can not afford that"
                    run (ctx, state)
            | None ->
                printfn $"No report for stock price `{symbol}` on date `{state.Date}`."
                run (ctx, state)
        | "sell" ->
            printf "Symbol > "
            let symbol = Console.ReadLine()
            printf "Amount > "

            let amount =
                Console.ReadLine()
                |> Decimal.TryParse
                |> fun r ->
                    match r with
                    | true, v -> v
                    | false, _ -> 0m

            let (stock, other) =
                state.Stocks
                |> List.partition (fun s -> String.Equals(s.Symbol, symbol, StringComparison.OrdinalIgnoreCase))

            match Store.getStockForDate ctx state.Date symbol, stock.IsEmpty |> not with
            | Some data, true ->

                match stock |> List.sumBy (fun s -> s.Amount) >= amount with
                | true ->

                    let value = data.OpenValue * amount

                    let newStock =
                        stock
                        |> List.fold
                            (fun (remaining, acc) s ->
                                let newRemaining = remaining - s.Amount

                                let stockRemaining =
                                    match s.Amount > remaining with
                                    | true -> s.Amount - remaining
                                    | false -> 0m

                                match (remaining > 0m) with
                                | true ->
                                    printfn $"{s.Amount - stockRemaining} stock sold."
                                    printfn $"\tBuy price: {s.BuyPrice}"
                                    printfn $"\tSell price: {data.OpenValue}"
                                    printfn $"\tProfit: {data.OpenValue - s.BuyPrice}"
                                    printfn $"\tDays: {(s.BroughtOn - state.Date).TotalDays}"
                                | false -> ()

                                newRemaining, acc @ [ { s with Amount = stockRemaining } ])
                            (amount, [])
                        |> fun (_, s) -> s |> List.filter (fun s -> s.Amount > 0m)


                    printfn $"Sold {amount} `{symbol}` stock for {value}"

                    let newState =
                        { state with
                            Credit = state.Credit + value
                            Stocks = other @ newStock }

                    run (ctx, newState)
                | false ->
                    printfn $"You do not own enough of `{symbol}` stock."
                    run (ctx, state)
            | None, _ ->
                printfn $"No data for date"
                run (ctx, state)
            | _, false ->
                printfn $"You own none of `{symbol}` stock."
                run (ctx, state)
        | "performance" ->
            state.Stocks
            |> List.iter (fun stock ->
                match getReport ctx state.Date stock.Symbol with
                | Some report ->
                    let diff = report.Open - stock.BuyPrice
                    let value = diff * stock.Amount
                    printReport ctx report

                    printfn $"{diff} (value: {value})"
                | None -> printfn $"No report for symbol `{stock.Symbol}` on date `{state.Date}`.")

            run (ctx, state)
        | "state" ->
            printfn $"{state}"
            run (ctx, state)
        | "advance" ->
            printf "Amount > "

            let amount =
                Console.ReadLine()
                |> Double.TryParse
                |> fun r ->
                    match r with
                    | true, v -> v
                    | false, _ -> 0.

            run (ctx, { state with Date = state.Date.AddDays(amount) })

        | "next" -> run (ctx, { state with Date = state.Date.AddDays(1.) })
        | "exit" -> state
        | "chart" ->
            printf "Symbol > "
            let symbol = Console.ReadLine()

            let path =
                "C:\\ProjectData\\fstock\\test_chart.svg"

            Persistence.Operations.selectStockRecords
                ctx
                [ "WHERE symbol = @0 AND Date(entry_date) >= DATE(@1) AND DATE(entry_date) < DATE(@2)" ]
                [ symbol
                  state.Date.AddDays(-31.)
                  state.Date.AddDays(-1.) ]
            |> List.map (fun s ->
                let prev200 =
                    Store.previousXDays ctx 200 s.EntryDate s.Symbol

                let prev50 =
                    Store.previousXDays ctx 50 s.EntryDate s.Symbol

                ({ Date = s.EntryDate
                   Open = s.OpenValue
                   Close = s.CloseValue
                   High = s.HighValue
                   Low = s.LowValue
                   AdjClose = s.AdjustedCloseValue
                   Volume = s.VolumeValue
                   MovingAverage200 = prev200 |> List.averageBy (fun s -> s.CloseValue)
                   MovingAverage50 = prev50 |> List.averageBy (fun s -> s.CloseValue) }: DataPoint))
            |> generateCandleStickChart ({ ValuePaddingPercent = 20m })
            |> saveToFile path 120 100

            printfn $"Chart `{path}` saved."
            run (ctx, state)
        | "clear" ->
            Console.Clear()
            run (ctx, state)
        | _ ->
            printfn $"Unknown command `{command}`."
            run (ctx, state)





    let start (date: DateTime) (credit: decimal) =

        let ctx =
            SqliteContext.Open "E:\\data\\stock_market\\fstock_store.db"

        let state =
            ({ Date = date
               Stocks = []
               Credit = credit }: State)

        run (ctx, state)


let date = DateTime(2019, 2, 5)

let endState = Simulator.start date 1000m

(*
let amount = 100m

let ctx =
    SqliteContext.Open "E:\\data\\stock_market\\fstock_store.db"

match getReport ctx date "TSLA" with
| Some report -> printReport ctx report
| None -> ()

let startRecord =
    Store.getStockForDate ctx date "TSLA"

let endRecord =
    Store.getStockForDate ctx (date.AddDays(50.)) "TSLA"

match startRecord, endRecord with
| Some sr, Some er ->
    // Brought for
    let maxBuyCost = sr.HighValue * amount
    let minBuyCost = sr.LowValue * amount
    let maxSellValue = er.HighValue * amount
    let minSellValue = er.LowValue * amount

    printfn $"Start date: {sr.EntryDate}"
    printfn $"End date: {er.EntryDate}"
    printfn $"Max buy cost: {maxBuyCost}"
    printfn $"Min by cost: {minBuyCost}"
    printfn $"Max sell value: {maxSellValue}"
    printfn $"Min sell value: {minSellValue}"
    printfn $"Best case: {maxSellValue - minBuyCost}"
    printfn $"Worse case: {minSellValue - maxBuyCost}"
| _ -> ()
*)


printfn "Hello from F#"
