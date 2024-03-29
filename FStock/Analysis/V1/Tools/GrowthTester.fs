﻿namespace FStock.Analysis.V1.Tools

open System
open Freql.Sqlite
open Microsoft.FSharp.Core

/// <summary>
/// Used to test growth of a position over time.
/// This can be used to test the validity of predictions based on various analysis
/// </summary>
[<RequireQualifiedAccess>]
module GrowthTester =

    open FStock.Data.Persistence

    type Parameters =
        { StartDate: DateTime
          Condition: Condition
          FetchHandler: FetchType }

    and FetchType =
        | Individual of (string -> DateTime -> decimal Option)
        | Group of (string -> FetchedGroupItem list)

    and FetchedGroupItem = { Date: DateTime; Value: decimal }

    and [<RequireQualifiedAccess>] Condition =
        | TakeProfit of Percent: decimal
        | LossStop of Percent: decimal
        | FixedPeriod of PeriodSize: int
        | Bespoke of Name: string * Fn: (decimal -> decimal -> bool)
        | And of Condition * Condition
        | Or of Condition * Condition
        | Any of Condition list
        | All of Condition list

        member c.Test(startingPrice: decimal, currentPrice: decimal, periodCount: int) =
            match c with
            | Condition.TakeProfit percent ->
                match ((startingPrice / 100m) * percent) + startingPrice < currentPrice with
                | true -> ConditionTestResult.True("take-profit", $"take profit ({percent}%%) hit")
                | false -> ConditionTestResult.False
            | Condition.LossStop percent ->
                match startingPrice - ((startingPrice / 100m) * percent) > currentPrice with
                | true -> ConditionTestResult.True("loss-stop", $"loss stop ({percent}%%) hit")
                | false -> ConditionTestResult.False
            | Condition.FixedPeriod periodSize ->
                match periodCount >= periodSize with
                | true -> ConditionTestResult.True("fixed-period", $"fixed period size ({periodSize}) hit")
                | false -> ConditionTestResult.False
            | Condition.Bespoke(name, fn) ->
                match fn startingPrice currentPrice with
                | true -> ConditionTestResult.True(name, $"bespoke condition {name} hit")
                | false -> ConditionTestResult.False
            | Condition.And(condition, condition1) ->
                match condition.Test(startingPrice, currentPrice, periodCount) with
                | ConditionTestResult.True(name, message) as ctr ->
                    match condition1.Test(startingPrice, currentPrice, periodCount) with
                    | ConditionTestResult.True(name1, message1) ->
                        ConditionTestResult.True($"{name}_and_{name1}", [ message; message1 ] |> String.concat " and ")
                    | ConditionTestResult.False -> ConditionTestResult.False
                | ConditionTestResult.False -> ConditionTestResult.False
            | Condition.Or(condition, condition1) ->
                match condition.Test(startingPrice, currentPrice, periodCount) with
                | ConditionTestResult.True _ as ctr -> ctr
                | ConditionTestResult.False -> condition1.Test(startingPrice, currentPrice, periodCount)
            | Condition.Any conditions ->
                conditions
                |> List.fold
                    (fun r c ->
                        match r with
                        | ConditionTestResult.True _ -> r
                        | ConditionTestResult.False -> c.Test(startingPrice, currentPrice, periodCount))
                    ConditionTestResult.False
            | Condition.All conditions ->
                conditions
                |> List.fold
                    (fun (r: Result<(string * string) list, unit>) c ->
                        match r with
                        | Ok acc ->
                            match c.Test(startingPrice, currentPrice, periodCount) with
                            | ConditionTestResult.True(name, message) as ctr -> Ok(acc @ [ name, message ])
                            | ConditionTestResult.False -> Error()
                        | Error _ -> r)
                    (Result.Ok [])
                |> fun r ->
                    match r with
                    | Ok rs ->
                        let namePaths, messageParts = rs |> List.unzip

                        ConditionTestResult.True(
                            namePaths |> String.concat "_and_",
                            messageParts |> String.concat " and "
                        )
                    | Error _ -> ConditionTestResult.False

    and [<RequireQualifiedAccess>] ConditionTestResult =
        | True of Name: string * Message: string
        | False

    and TestResult =
        | Finished of TestResultReport
        | NoResult of Symbol: string

        member tr.Serialize() =
            match tr with
            | Finished testResultReport ->
                let diff = testResultReport.SellPrice - testResultReport.BuyPrice

                let percentDiff = ((diff) / testResultReport.BuyPrice) * 100m

                $"{testResultReport.Symbol} - buy: {testResultReport.BuyPrice} sell: {testResultReport.SellPrice} diff: {diff} ({percentDiff}%%) len: {(testResultReport.EndDate - testResultReport.StartDate).TotalDays} (message: {testResultReport.ConditionMessage})"
            | NoResult symbol -> $"{symbol} - [No result]"

    and TestResultReport =
        { Symbol: string
          StartDate: DateTime
          EndDate: DateTime
          BuyPrice: decimal
          SellPrice: decimal
          ConditionName: string
          ConditionMessage: string }

    [<CLIMutable>]
    type TableListingItem =
        { Name: string
          StartDate: DateTime
          TakeProfit: decimal option
          LossStop: decimal option
          MaxPeriods: int option }

    let run (parameters: Parameters) (symbol: string) (startPrice: decimal) =

        match parameters.FetchHandler with
        | Individual fn ->
            let rec handler (date: DateTime) (currentPeriod: int) =
                match DateTime.UtcNow.Date <= date with
                | true -> TestResult.NoResult symbol
                | false ->
                    match fn symbol date with
                    | Some cp ->
                        match parameters.Condition.Test(startPrice, cp, currentPeriod) with
                        | ConditionTestResult.True(name, message) ->

                            { Symbol = symbol
                              StartDate = parameters.StartDate
                              EndDate = date
                              BuyPrice = startPrice
                              SellPrice = cp
                              ConditionName = name
                              ConditionMessage = message }
                            |> TestResult.Finished
                        | ConditionTestResult.False -> handler (date.AddDays(1)) (currentPeriod + 1)
                    | None -> handler (date.AddDays(1)) (currentPeriod + 1)

            handler parameters.StartDate (1)
        | Group fn ->
            let rec handler (currentPeriod: int) (items: FetchedGroupItem list) =
                match items.IsEmpty with
                | true -> TestResult.NoResult symbol
                | false ->
                    let (head, tail) = items.Head, items.Tail

                    match parameters.Condition.Test(startPrice, head.Value, currentPeriod) with
                    | ConditionTestResult.True(name, message) ->
                        { Symbol = symbol
                          StartDate = parameters.StartDate
                          EndDate = head.Date
                          BuyPrice = startPrice
                          SellPrice = head.Value
                          ConditionName = name
                          ConditionMessage = message }
                        |> TestResult.Finished
                    | ConditionTestResult.False -> handler (currentPeriod + 1) tail

            fn symbol |> handler 1


    let runAndSave
        (ctx: SqliteContext)
        (symbol: string)
        (startDate: DateTime)
        (takeProfit: decimal option)
        (stopLoss: decimal option)
        (periodCount: int option)
        =
        let tableNameSuffix =
            match takeProfit, stopLoss with
            | Some tp, Some sl -> "tp_{}_sl_{}"
            
            | None, Some sl -> $"tp_no_sl_{sl}"
            | Some value, None -> failwith "todo"
            | None, None -> "tp_no_sl_no"


        ()
