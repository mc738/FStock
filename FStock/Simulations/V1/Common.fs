namespace FStock.Simulations.V1

open System

[<AutoOpen>]
module Common =

    type OpenPosition =
        { Id: string
          ParentId: string option
          Symbol: string
          Start: DateTime
          BuyPrice: decimal
          Volume: decimal }

    type ClosedPosition =
        { Id: string
          ParentId: string option
          Symbol: string
          BuyDate: DateTime
          SellDate: DateTime
          BuyPrice: decimal
          SellPrice: decimal
          Volume: decimal }

    type CurrentPosition =
        { Symbol: string
          Date: DateTime
          Open: decimal
          High: decimal
          Low: decimal
          Close: decimal
          AdjustedClose: decimal }

    type Portfolio =
        { OpenPositions: OpenPosition list
          ClosedPositions: ClosedPosition list }

        member p.Buy(symbol, date, price, volume) =
            { p with
                OpenPositions =
                    p.OpenPositions
                    @ [ { Id = System.Guid.NewGuid().ToString("n")
                          ParentId = None
                          Symbol = symbol
                          Start = date
                          BuyPrice = price
                          Volume = volume } ] }

        member p.GetOpenPositionsForSymbol(symbol) =
            p.OpenPositions |> List.filter (fun op -> op.Symbol = symbol)

        member p.TrySell(id, date, price, volume) =
            // Find the position
            let (fop, oop) = p.OpenPositions |> List.partition (fun op -> op.Id = id)

            match fop |> List.tryHead with
            | Some op ->
                // If
                match volume < op.Volume with
                | true ->
                    // If volume is less then total volume create an open and closed position.
                    let nop =
                        ({ Id = System.Guid.NewGuid().ToString()
                           ParentId = Some op.Id
                           Symbol = op.Symbol
                           Start = op.Start
                           BuyPrice = op.BuyPrice
                           Volume = op.Volume - volume }
                        : OpenPosition)

                    let ncp =
                        ({ Id = op.Id
                           ParentId = op.ParentId
                           Symbol = op.Symbol
                           BuyDate = op.Start
                           SellDate = date
                           BuyPrice = op.BuyPrice
                           SellPrice = price
                           Volume = volume }
                        : ClosedPosition)

                    ({ p with
                        OpenPositions = oop @ [ nop ]
                        ClosedPositions = p.ClosedPositions @ [ ncp ] })
                    |> Ok
                | false ->
                    // Just need a new close position
                    // NOTE if volume is more than is held this will handle it correctly.

                    let ncp =
                        ({ Id = op.Id
                           ParentId = op.ParentId
                           Symbol = op.Symbol
                           BuyDate = op.Start
                           SellDate = date
                           BuyPrice = op.BuyPrice
                           SellPrice = price
                           Volume = op.Volume }
                        : ClosedPosition)

                    ({ p with
                        OpenPositions = oop
                        ClosedPositions = p.ClosedPositions @ [ ncp ] })
                    |> Ok
            | None -> Error "Position not found"

        member p.GetClosedPositionsForSymbol(symbol) =
            p.ClosedPositions |> List.filter (fun cp -> cp.Symbol = symbol)

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
        | Bespoke of Handler: (OpenPosition -> CurrentPosition -> bool)
        | And of PositionCondition * PositionCondition
        | Or of PositionCondition * PositionCondition
        | All of PositionCondition list
        | Any of PositionCondition list
        | Not of PositionCondition

        member pc.Test(position: OpenPosition, current: CurrentPosition, settings: SimulationSettings) =
            let currentValue =
                match settings.ValueMode with
                | ValueMode.Open -> current.Open
                | ValueMode.Close -> current.Close
                | ValueMode.High -> current.High
                | ValueMode.Low -> current.Low
                | ValueMode.AdjustedClose -> current.AdjustedClose

            let rec handle (condition: PositionCondition) =
                match condition with
                | PercentageGrowth percent ->
                    ((currentValue - position.BuyPrice) / (position.BuyPrice) * 100m) >= percent
                | FixedValue value -> currentValue >= value
                | PercentageLoss percent -> (((position.BuyPrice - currentValue) / position.BuyPrice) * 100m) >= percent
                | FixedLoss value -> currentValue <= value
                | Duration length -> (current.Date - position.Start).Days >= length
                | Bespoke handler -> handler position current
                | And(a, b) -> handle a && handle b
                | Or(a, b) -> handle a || handle b
                | All positionConditions -> positionConditions |> List.exists (fun c -> handle c |> not) |> not
                | Any positionConditions -> positionConditions |> List.exists handle
                | Not positionCondition -> handle positionCondition |> not

            handle pc

    type PositionAction =
        | IncreasePositionByFixedAmount of Amount: decimal
        | IncreasePositionByPercentage of Percent: decimal
        | DecreasePositionByFixedAmount of Amount: decimal
        | DecreasePositionByPercentage of Percent: decimal

    type PositionBehaviour =
        { Condition: PositionCondition
          Action: PositionAction }

    type TradingModel =
        { Behaviours: Map<string, PositionBehaviour>
          GeneralBehaviours: PositionBehaviour list
          DefaultBehaviour: PositionCondition }

    type SimulationContext =
        { Portfolio: Portfolio
          Model: TradingModel }
