﻿namespace FStock.Analysis.V1.Tools

open System

/// <summary>
/// Used to test growth of a position over time.
/// This can be used to test the validity of predictions based on various analysis
/// </summary>
[<RequireQualifiedAccess>]
module GrowthTester =

    open FStock.Data.Persistence

    type Parameters =
        { StartDate: DateTime
          Test: Condition
          FetchHandler: string -> DateTime -> decimal Option }

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
                match startingPrice - ((startingPrice / 100m) * percent) > startingPrice with
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
        | True of Type: string * Message: string
        | False

    and TestResult =
        | Finished
        | NoResult

    and TestResultReport =
        { Symbol: string
          StartDate: DateTime
          EndDate: DateTime
          BuyPrice: decimal
          SellPrice: decimal }


    let run () =
        
        ()

    ()
