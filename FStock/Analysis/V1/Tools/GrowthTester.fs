namespace FStock.Analysis.V1.Tools

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
          FetchHandler: Records.Stock -> DateTime -> Records.Stock Option }

    and Condition =
        | TakeProfit of Percent: decimal
        | LossStop of Percent: decimal
        | FixedPeriod of PeriodSize: int
        | Bespoke of Fn: (Records.Stock -> Records.Stock -> bool)
        | And of Condition * Condition
        | Or of Condition * Condition
        | Any of Condition list
        | All of Condition list

    and RunUntil =
        | 
    
    and TestResult =
        | Finished
        | NoResult

    and TestResultReport =
        { Symbol: string
          StartDate: DateTime
          EndDate: DateTime
          BuyPrice: decimal
          SellPrice: decimal }



    ()
