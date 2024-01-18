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

    and LogItem = { Message: string }

    type SimulationContext =
        { Portfolio: Portfolio
          Model: TradingModel
          CurrentPositionHandler: OpenPosition -> DateTime -> CurrentPosition
          Settings: SimulationSettings
          BehaviourMaps: BehaviourMap list }

        member ctx.Handle(date: DateTime) =
            // First find all open positions and behaviours and related behaviours.

            let getValue (cp: CurrentPosition) =
                match ctx.Settings.ValueMode with
                | ValueMode.Open -> cp.Open
                | ValueMode.Close -> cp.Close
                | ValueMode.High -> cp.High
                | ValueMode.Low -> cp.Low
                | ValueMode.AdjustedClose -> failwith "todo"
            
            ctx.Portfolio.OpenPositions
            |> List.map (fun op ->
                op,
                ctx.BehaviourMaps
                |> List.filter (fun bm -> bm.PositionId = op.Id)
                |> List.sortBy (fun bm -> bm.Priority) // This will prioritize behaviours.
                |> List.choose (fun bm ->
                    ctx.Model.Behaviours.TryFind bm.BehaviourId
                    |> Option.map (fun b -> b, bm.Priority)))
            |> List.map (fun (op, bs) ->
                // Get the current position.
                let cp = ctx.CurrentPositionHandler op date

                // Now prioritize behaviours.

                // Then run through them until one is triggered.
                bs
                |> List.sortBy snd
                |> List.fold
                    (fun (pa: PrioritizedActions list) (b, p) ->
                        match b.Condition.Test(op, cp, ctx.Settings) with
                        | true -> pa @ [ { Actions = b.Actions; Priority = p } ]
                        | false -> pa)
                    []
                |> fun pas ->
                    // Combine actions so they are simplified.
                    // For example if there is action to buy 10 and sell 5 then this will be combined to buy 5.
                    match ctx.Settings.ActionCombinationMode with
                    | ActionCombinationMode.Simple ->
                        // Simple mode -> first actions hit
                        match pas.IsEmpty |> not with
                        | true -> pas.Head.Actions
                        | false -> []
                    | ActionCombinationMode.Bespoke fn -> fn pas
                |> fun ta ->
                    { Position = op
                      Actions = ta
                      CurrentPosition = cp })
            |> List.fold
                (fun (state: Portfolio, acc) (ta) ->
                    ta.Actions
                    |> List.fold
                        (fun (p: Portfolio, innerAcc) a ->
                            match a with
                            | PositionAction.IncreasePositionByFixedAmount amount ->
                                
                                
                                p.Buy(ta.Position.Symbol, date, "", amount), [ "" ]
                            | PositionAction.IncreasePositionByPercentage percent ->
                                
                                
                                failwith "todo"
                            | PositionAction.DecreasePositionByFixedAmount amount -> failwith "todo"
                            | PositionAction.DecreasePositionByPercentage percent -> failwith "todo")
                        (state, acc))
                (ctx.Portfolio, [])
// Update portfolio/behaviours based on the actions.
