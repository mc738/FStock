namespace FStock.Modeling.V1

open System

[<RequireQualifiedAccess>]
module Backtesting =

    type ModelState =
        { Portfolio: Portfolio
          Model: TradingModel
          StartDate: DateTime
          CurrentDate: DateTime
          BehaviourMaps: BehaviourMap list }

    and UpdateState =
        { Portfolio: Portfolio
          BehaviourMaps: BehaviourMap list
          Logs: string list }

        static member Create(portfolio, behaviourMaps) =
            { Portfolio = portfolio
              BehaviourMaps = behaviourMaps
              Logs = [] }

        member us.Buy(symbol, date, price, volume, investmentType) =
            match us.Portfolio.TryBuy(symbol, date, price, volume, investmentType) with
            | BuyResult.Success(newPortfolio, newId) ->
                { us with
                    Portfolio = newPortfolio
                    Logs = us.Logs @ [ $"Brought {volume} at {price} of {symbol}" ] }
                |> fun ns -> UpdateStateResult.Success(ns, Some newId)
            | BuyResult.Failure message ->
                { us with
                    Logs =
                        us.Logs
                        @ [ $"Failed to buy {volume} at {price} of {symbol}. Result: {message}" ] }
                |> fun ns -> UpdateStateResult.Failure(ns, message)

        member us.Sell(symbol, date, price, volume) =

            match us.Portfolio.TrySell(symbol, date, price, volume) with
            | SellResult.Success(newPortfolio, newId) ->
                { us with
                    Portfolio = newPortfolio
                    Logs = us.Logs @ [ $"Sold {volume} at {price} of {symbol}" ] }
                |> fun ns -> UpdateStateResult.Success(ns, newId)
            | SellResult.Failure message ->
                { us with
                    Logs =
                        us.Logs
                        @ [ $"Failed to buy {volume} at {price} of {symbol}. Result: {message}" ] }
                |> fun ns -> UpdateStateResult.Failure(ns, message)

    and [<RequireQualifiedAccess>] UpdateStateResult =
        | Success of NewState: UpdateState * NewPositionId: string option
        | Failure of NewState: UpdateState * Message: string

    type NewStateRewriter = UpdateState -> UpdateState

    type TestingSettings =
        { BuyValueMode: ValueMode
          SellValueMode: ValueMode
          ActionCombinationMode: ActionCombinationMode }

    type TestingContext =
        { Settings: TestingSettings
          CurrentPositionHandler: OpenPosition -> DateTime -> CurrentPosition option
          HistoricPositionsHandler: HistoricPositionFilter -> HistoricPosition list
          NewStateRewriters: NewStateRewriter list }

    [<RequireQualifiedAccess>]
    type NewStateResult = Changed of NewState: ModelState * Log: string list

    let testCondition
        (ctx: TestingContext)
        (state: ModelState)
        (position: OpenPosition)
        (current: CurrentPosition)
        (condition: PositionCondition)
        =
        let rec handle (condition: PositionCondition) =
            match condition with
            | PercentageGrowth(percent, valueMapper) ->
                ((valueMapper.GetValue current - position.BuyPrice) / (position.BuyPrice) * 100m)
                >= percent
            | FixedValue(value, valueMapper) -> valueMapper.GetValue current >= value
            | PercentageLoss(percent, valueMapper) ->
                (((position.BuyPrice - valueMapper.GetValue current) / position.BuyPrice) * 100m)
                >= percent
            | FixedLoss(value, valueMapper) -> valueMapper.GetValue current <= value
            | Duration length -> (current.Date - position.Start).Days >= length
            | Bespoke handler -> handler position current ctx.HistoricPositionsHandler state.Portfolio
            | And(a, b) -> handle a && handle b
            | Or(a, b) -> handle a || handle b
            | All positionConditions -> positionConditions |> List.exists (fun c -> handle c |> not) |> not
            | Any positionConditions -> positionConditions |> List.exists handle
            | Not positionCondition -> handle positionCondition |> not

        handle condition

    let fetchBehaviours (state: ModelState) (position: OpenPosition) =
        state.BehaviourMaps
        |> List.filter (fun bm -> bm.PositionId = position.Id)
        |> List.sortBy (fun bm -> bm.Priority) // This will prioritize behaviours.
        |> List.choose (fun bm ->
            state.Model.Behaviours.TryFind bm.BehaviourId
            |> Option.map (fun b ->
                { Behaviour = b
                  Priority = bm.Priority }))

    let handleBehaviours
        (ctx: TestingContext)
        (state: ModelState)
        (date: DateTime)
        (position: OpenPosition)
        (behaviours: PrioritizedBehaviour list)
        =
        // Get the current position.
        ctx.CurrentPositionHandler position date

        |> Option.bind (fun cp ->
            // Now prioritize behaviours.

            // Then run through them until one is triggered.
            behaviours
            |> List.sortBy (fun pb -> pb.Priority)
            |> List.fold
                (fun (pa: PrioritizedActions list) pb ->
                    // NOTE currently this has no way of checking what other actions have been triggered.
                    // This does mean it works on first come/first serve basis
                    match testCondition ctx state position cp pb.Behaviour.Condition with
                    | true ->
                        pa
                        @ [ { Actions = pb.Behaviour.Actions
                              Priority = pb.Priority } ]
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
                { Position = position
                  Actions = ta
                  CurrentPosition = cp }
                |> Some)

    let handleActions (ctx: TestingContext) (state: ModelState) (date: DateTime) (actions: TriggeredActions list) =
        actions
        |> List.fold
            (fun (state: UpdateState) (ta) ->
                ta.Actions
                |> List.fold
                    (fun (us: UpdateState) a ->
                        match a with
                        | PositionAction.IncreasePositionByFixedAmount(amount, investmentType) ->
                            match
                                us.Buy(
                                    ta.Position.Symbol,
                                    date,
                                    ta.CurrentPosition.GetValue ctx.Settings.BuyValueMode,
                                    amount,
                                    investmentType
                                )
                            with
                            | UpdateStateResult.Success(ns, newPositionId) ->
                                // TODO add behaviours to new position

                                ns
                            | UpdateStateResult.Failure(ns, msg) -> ns
                        | PositionAction.IncreasePositionByPercentage(percent, investmentType) ->
                            let volumeIncrease = (ta.Position.Volume / 100m) * percent

                            match
                                us.Buy(
                                    ta.Position.Symbol,
                                    date,
                                    ta.CurrentPosition.GetValue ctx.Settings.BuyValueMode,
                                    volumeIncrease,
                                    investmentType
                                )
                            with
                            | UpdateStateResult.Success(ns, newPositionId) ->
                                // TODO add behaviours to new position

                                ns
                            | UpdateStateResult.Failure(ns, msg) -> ns
                        | PositionAction.DecreasePositionByFixedAmount amount ->
                            // TODO pass on action mappings to new position.
                            match
                                us.Sell(
                                    ta.Position.Id,
                                    date,
                                    ta.CurrentPosition.GetValue ctx.Settings.SellValueMode,
                                    amount
                                )
                            with
                            | UpdateStateResult.Success(ns, newPositionId) ->
                                // Copy across behaviours for new position.
                                match newPositionId with
                                | Some npi ->
                                    ns.BehaviourMaps
                                    |> List.filter (fun bm -> bm.PositionId = ta.Position.Id)
                                    |> List.map (fun bm -> { bm with PositionId = npi })
                                    |> fun nbm ->
                                        { ns with
                                            BehaviourMaps = ns.BehaviourMaps @ nbm }
                                | None -> ns
                            | UpdateStateResult.Failure(ns, msg) -> ns
                        | PositionAction.DecreasePositionByPercentage percent ->
                            let volumeDecrease = (ta.Position.Volume / 100m) * percent

                            match
                                us.Sell(
                                    ta.Position.Id,
                                    date,
                                    ta.CurrentPosition.GetValue ctx.Settings.BuyValueMode,
                                    volumeDecrease
                                )
                            with
                            | UpdateStateResult.Success(ns, newPositionId) ->
                                match newPositionId with
                                | Some npi ->
                                    ns.BehaviourMaps
                                    |> List.filter (fun bm -> bm.PositionId = ta.Position.Id)
                                    |> List.map (fun bm -> { bm with PositionId = npi })
                                    |> fun nbm ->
                                        { ns with
                                            BehaviourMaps = ns.BehaviourMaps @ nbm }
                                | None -> ns
                            | UpdateStateResult.Failure(ns, msg) -> ns)
                    state)
            (UpdateState.Create(state.Portfolio, state.BehaviourMaps))

    let rewriteState (ctx: TestingContext) (state: UpdateState) =
        ctx.NewStateRewriters |> List.fold (fun ns r -> r ns) state

    let progressState (ctx: TestingContext) (date: DateTime) (state: ModelState) =
        state.Portfolio.OpenPositions
        |> List.map (fun op -> op, fetchBehaviours state op)
        |> List.choose (fun (op, pbs) -> handleBehaviours ctx state date op pbs)
        |> (handleActions ctx state date)
        |> rewriteState ctx
        |> fun ns ->
            NewStateResult.Changed(
                { state with
                    Portfolio = ns.Portfolio
                    BehaviourMaps = ns.BehaviourMaps },
                ns.Logs
            )
