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
        
        member c.Test(startingPrice: decimal, currentPrice: decimal) =
            match c with
            | Condition.TakeProfit percent ->
                match 
                
                failwith "todo"
            | Condition.LossStop percent -> failwith "todo"
            | Condition.FixedPeriod periodSize -> failwith "todo"
            | Condition.Bespoke(name, fn) -> failwith "todo"
            | Condition.And(condition, condition1) -> failwith "todo"
            | Condition.Or(condition, condition1) -> failwith "todo"
            | Condition.Any conditions -> failwith "todo"
            | Condition.All conditions -> failwith "todo"
            
    and [<RequireQualifiedAccess>] ConditionTestResult =
        | True of Type: string * Message : string
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



    ()
