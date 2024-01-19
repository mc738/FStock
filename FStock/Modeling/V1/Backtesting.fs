namespace FStock.Modeling.V1

open System

module Backtesting =

    type ModelState =
        { Portfolio: Portfolio
          Model: TradingModel

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
          NewStateRewriters: NewStateRewriter list  }

    let progressState (state: ModelState) =
        state.Portfolio.OpenPositions
        
        
        
        ()
    
    
    

    ()
