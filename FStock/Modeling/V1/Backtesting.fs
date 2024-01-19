namespace FStock.Modeling.V1

open System

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

        member us.Buy(symbol, date, price, volume) =
            { us with
                Portfolio = us.Portfolio.Buy(symbol, date, price, volume)
                Logs = us.Logs @ [ "" ] }

        member us.Sell(symbol, date, price, volume) =

            match us.Portfolio.TrySell(symbol, date, price, volume) with
            | Ok np ->
                { us with
                    Portfolio = np
                    Logs = us.Logs @ [ "" ] }
            | Error e -> { us with Logs = us.Logs @ [ "" ] }

    type NewStateRewriter = UpdateState -> UpdateState

    type TestingContext =
        { Settings: SimulationSettings
          CurrentPositionHandler: OpenPosition -> DateTime -> CurrentPosition
          NewStateRewriters: NewStateRewriter list }

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
        (date: DateTime)
        (position: OpenPosition)
        (behaviours: PrioritizedBehaviour list)
        =
        // Get the current position.
        let cp = ctx.CurrentPositionHandler position date

        // Now prioritize behaviours.

        // Then run through them until one is triggered.
        behaviours
        |> List.sortBy (fun pb -> pb.Priority)
        |> List.fold
            (fun (pa: PrioritizedActions list) pb ->
                match pb.Behaviour.Condition.Test(position, cp, ctx.Settings) with
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

    let handleActions (ctx: TestingContext) (state: ModelState) (date: DateTime) (actions: TriggeredActions list) =
        actions
        |> List.fold
            (fun (state: UpdateState) (ta) ->
                ta.Actions
                |> List.fold
                    (fun (us: UpdateState) a ->
                        match a with
                        | PositionAction.IncreasePositionByFixedAmount amount ->
                            us.Buy(ta.Position.Symbol, date, getValue ta.CurrentPosition, amount)
                        | PositionAction.IncreasePositionByPercentage percent ->
                            let volumeIncrease = (ta.Position.Volume / 100m) * percent

                            us.Buy(ta.Position.Symbol, date, getValue ta.CurrentPosition, volumeIncrease)
                        | PositionAction.DecreasePositionByFixedAmount amount ->
                            // TODO pass on action mappings to new position.
                            us.Sell(ta.Position.Id, date, getValue ta.CurrentPosition, amount)
                        | PositionAction.DecreasePositionByPercentage percent ->
                            let volumeDecrease = (ta.Position.Volume / 100m) * percent
                            us.Sell(ta.Position.Id, date, getValue ta.CurrentPosition, volumeDecrease))
                    state)
            (UpdateState.Create(state.Portfolio, state.BehaviourMaps))

    let rewriteState (ctx: TestingContext) (state: UpdateState) =
        ctx.NewStateRewriters |> List.fold (fun ns r -> r ns) state

    let progressState (ctx: TestingContext) (state: ModelState) (date: DateTime) =
        state.Portfolio.OpenPositions
        |> List.map (fun op -> op, fetchBehaviours state op)
        |> List.map (fun (op, pbs) -> handleBehaviours ctx date op pbs)
        |> (handleActions ctx state date)
        |> rewriteState ctx






    ()
