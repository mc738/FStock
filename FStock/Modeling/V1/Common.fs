﻿namespace FStock.Modeling.V1

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

    and ClosedPosition =
        { Id: string
          ParentId: string option
          Symbol: string
          BuyDate: DateTime
          SellDate: DateTime
          BuyPrice: decimal
          SellPrice: decimal
          Volume: decimal }

    and CurrentPosition =
        { Symbol: string
          Date: DateTime
          Open: decimal
          High: decimal
          Low: decimal
          Close: decimal
          AdjustedClose: decimal }

        member cp.GetValue(mode: ValueMode) =
            match mode with
            | ValueMode.Open -> cp.Open
            | ValueMode.Close -> cp.Close
            | ValueMode.High -> cp.High
            | ValueMode.Low -> cp.Low
            | ValueMode.AdjustedClose -> cp.AdjustedClose

    and HistoricPosition =
        { Symbol: string
          Date: DateTime
          Open: decimal
          High: decimal
          Low: decimal
          Close: decimal
          AdjustedClose: decimal }

    and HistoricPositionFilter =
        { From: DateTime option
          FromInclusive: bool
          To: DateTime option
          ToInclusive: bool
          SymbolFilter: SymbolFilter }

    and HistoricPositionHandler = HistoricPositionFilter -> HistoricPosition list
    
    and [<RequireQualifiedAccess>] SymbolFilter =
        | All
        | Stocks
        | Etfs
        | In of Symbols: string list
        | NotIn of Symbols: string list
        | EqualTo of Symbol: string
        | NotEqualTo of Symbol: string

    and Portfolio =
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

    and PositionCondition =
        | PercentageGrowth of Percent: decimal * ValueMapper: ConditionValueMapper
        | FixedValue of Value: decimal * ValueMapper: ConditionValueMapper
        | PercentageLoss of Percent: decimal * ValueMapper: ConditionValueMapper
        | FixedLoss of Value: decimal * ValueMapper: ConditionValueMapper
        | Duration of Length: int
        | Bespoke of Handler: (OpenPosition -> CurrentPosition -> HistoricPositionHandler -> bool)
        | And of PositionCondition * PositionCondition
        | Or of PositionCondition * PositionCondition
        | All of PositionCondition list
        | Any of PositionCondition list
        | Not of PositionCondition

    and ConditionValueMapper =
        | Value of Mode: ValueMode
        | FixedAdjustment of Mode: ValueMode * Adjustment: decimal
        | PercentageIncrease of Mode: ValueMode * Percentage: decimal
        | PercentageDecrease of Mode: ValueMode * Percentage: decimal

        member cvm.GetValue(position: CurrentPosition) =
            let getBaseValue (mode: ValueMode) =
                match mode with
                | ValueMode.Open -> position.Open
                | ValueMode.Close -> position.Close
                | ValueMode.High -> position.High
                | ValueMode.Low -> position.Low
                | ValueMode.AdjustedClose -> position.AdjustedClose

            match cvm with
            | Value mode -> getBaseValue mode
            | FixedAdjustment(mode, adjustment) -> getBaseValue mode + adjustment
            | PercentageIncrease(mode, percentage) ->
                let bv = getBaseValue mode

                bv + ((bv / 100m) * percentage)
            | PercentageDecrease(mode, percentage) ->
                let bv = getBaseValue mode

                bv - ((bv / 100m) * percentage)

    and [<RequireQualifiedAccess>] PositionAction =
        | IncreasePositionByFixedAmount of Amount: decimal
        | IncreasePositionByPercentage of Percent: decimal
        | DecreasePositionByFixedAmount of Amount: decimal
        | DecreasePositionByPercentage of Percent: decimal

    and PositionBehaviour =
        { Condition: PositionCondition
          Actions: PositionAction list }

    and TradingModel =
        { Behaviours: Map<string, PositionBehaviour>
          GeneralBehaviours: PositionBehaviour list
          DefaultBehaviour: PositionCondition }

    and BehaviourMap =
        { BehaviourId: string
          PositionId: string
          Priority: int }

    and PrioritizedBehaviour =
        { Behaviour: PositionBehaviour
          Priority: int }

    and PrioritizedActions =
        { Actions: PositionAction list
          Priority: int }

    and SimulationSettings =
        { ValueMode: ValueMode
          ActionCombinationMode: ActionCombinationMode }

    and [<RequireQualifiedAccess>] ValueMode =
        | Open
        | Close
        | High
        | Low
        | AdjustedClose

    and [<RequireQualifiedAccess>] ActionCombinationMode =
        | Simple
        | Bespoke of Handler: (PrioritizedActions list -> PositionAction list)

    and TriggeredActions =
        { Position: OpenPosition
          Actions: PositionAction list
          CurrentPosition: CurrentPosition }

    module Conditions =

        /// <summary>
        /// A behaviour to check the current position's low value vs a fixed lose.
        /// This is to simulate if a stop-loss has been hit.
        /// </summary>
        /// <param name="stopLoss"></param>
        let ``fixed stop-loss`` stopLoss =
            PositionCondition.FixedLoss(stopLoss, ConditionValueMapper.Value ValueMode.Low)

        /// <summary>
        /// A behaviour to check the current position's low value vs a percentage lose.
        /// This is to simulate if a stop-loss has been hit.
        /// </summary>
        /// <param name="stopLossPercent"></param>
        let ``percentage stop-loss`` stopLossPercent =
            PositionCondition.PercentageLoss(stopLossPercent, ConditionValueMapper.Value ValueMode.Low)

        let ``fixed take profit`` takeProfit =
            PositionCondition.FixedValue(takeProfit, ConditionValueMapper.Value ValueMode.High)

        let ``percentage take profit`` takeProfitPercent (actions: PositionAction list) =
            PositionCondition.PercentageGrowth(takeProfitPercent, ConditionValueMapper.Value ValueMode.High)