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

    type SimulationSettings = { ValueMode: ValueMode }

    and [<RequireQualifiedAccess>] ValueMode =
        | Open
        | Close
        | High
        | Low
        | AdjustedClose



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

        member pc.Test(start: PositionStart, current: CurrentPosition, settings: SimulationSettings) =
            let currentValue =
                match settings.ValueMode with
                | ValueMode.Open -> current.Open
                | ValueMode.Close -> current.Close
                | ValueMode.High -> current.High
                | ValueMode.Low -> current.Low
                | ValueMode.AdjustedClose -> current.AdjustedClose

            let rec handle (condition: PositionCondition) =
                match condition with
                | PercentageGrowth percent -> ((currentValue - start.BuyPrice) / (start.BuyPrice) * 100m) >= percent
                | FixedValue value -> currentValue >= value
                | PercentageLoss percent -> (((start.BuyPrice - currentValue) / start.BuyPrice) * 100m) >= percent
                | FixedLoss value -> currentValue <= value
                | Duration length -> (current.Date - start.Start).Days >= length
                | Bespoke handler -> handler start current
                | And(a, b) -> handle a && handle b
                | Or(a, b) -> handle a || handle b
                | All positionConditions -> positionConditions |> List.exists (fun c -> handle c |> not) |> not
                | Any positionConditions -> positionConditions |> List.exists handle
                | Not positionCondition -> handle positionCondition |> not

            handle pc
