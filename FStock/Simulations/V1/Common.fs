namespace FStock.Simulations.V1

open System

[<AutoOpen>]
module Common =

    type PositionStart =
        { Symbol: string
          Start: DateTime
          BuyPrice: decimal
          Volume: decimal }

    type CurrentPosition =
        { Symbol: string
          Date: DateTime
          Open: decimal
          High: decimal
          Low: decimal
          Close: decimal
          AdjustedClose: decimal }

    type Con

    type PositionCondition =
        | PercentageGrowth of Percent: decimal
        | FixedValue of Value: decimal
        | PercentageLoss of Percent: decimal
        | FixedLoss of Value: decimal
        | Duration of Length: int
        | Bespoke of Handler: (PositionStart -> CurrentPosition -> bool)
        | And of PositionCondition * PositionCondition
        | Or of PositionCondition * PositionCondition
        | All of PositionCondition list
        | Any of PositionCondition list
        | Not of PositionCondition

        member pc.Test(start: PositionStart, current: CurrentPosition, settings) =
            let rec handle (condition: PositionCondition) =
                match condition with
                | PercentageGrowth percent ->
                    
                    
                    failwith "todo"
                | FixedValue value -> failwith "todo"
                | PercentageLoss percent -> failwith "todo"
                | FixedLoss value -> failwith "todo"
                | Duration length -> failwith "todo"
                | Bespoke handler -> failwith "todo"
                | And(positionCondition, condition) -> failwith "todo"
                | Or(positionCondition, condition) -> failwith "todo"
                | All positionConditions -> failwith "todo"
                | Any positionConditions -> failwith "todo"
                | Not positionCondition -> failwith "todo"
                
            handle pc