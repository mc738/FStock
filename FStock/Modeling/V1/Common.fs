namespace FStock.Modeling.V1

open FStock.Data.Domain


[<AutoOpen>]
module Common =

    open System
    open FStock.Data.Domain
    
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
          AdjustedClose: decimal
          Volume: decimal }

        member cp.GetValue(mode: OHLCValue) =
            match mode with
            | OHLCValue.Open -> cp.Open
            | OHLCValue.Close -> cp.Close
            | OHLCValue.High -> cp.High
            | OHLCValue.Low -> cp.Low
            | OHLCValue.AdjustedClose -> cp.AdjustedClose
            | OHLCValue.Volume  -> cp.Volume

    and HistoricPosition =
        { Symbol: string
          Date: DateTime
          Open: decimal
          High: decimal
          Low: decimal
          Close: decimal
          AdjustedClose: decimal
          Volume: decimal }

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
        {
            /// <summary>
            /// The initial amount invested in the portfolio.
            /// This will only increase but will not found money reinvested.
            /// </summary>
            InitialInvestment: decimal
            Liquidity: decimal
            OpenPositions: OpenPosition list
            ClosedPositions: ClosedPosition list
        }

        member p.AddInitialInvestment(value: decimal) =
            { p with
                InitialInvestment = p.InitialInvestment + value
                Liquidity = p.Liquidity + value }

        member p.TryBuy(symbol, date, price, volume, ?investmentType) =
            let totalCost = price * volume

            match investmentType |> Option.defaultValue InvestmentType.PrioritizeLiquidity with
            | LiquidityOnly ->
                match p.Liquidity >= (totalCost) with
                | true ->
                    let newId = System.Guid.NewGuid().ToString("n")

                    { p with
                        Liquidity = p.Liquidity - totalCost
                        OpenPositions =
                            p.OpenPositions
                            @ [ { Id = newId
                                  ParentId = None
                                  Symbol = symbol
                                  Start = date
                                  BuyPrice = price
                                  Volume = volume } ] }
                    |> fun np -> BuyResult.Success(np, newId)
                | false -> BuyResult.Failure "Not enough liquidity"
            | PrioritizeLiquidity ->
                // How much can be done with liquidity? Any left over amount is new investment.
                let newId = System.Guid.NewGuid().ToString("n")

                match p.Liquidity >= totalCost with
                | true ->
                    { p with
                        Liquidity = p.Liquidity - totalCost
                        OpenPositions =
                            p.OpenPositions
                            @ [ { Id = newId
                                  ParentId = None
                                  Symbol = symbol
                                  Start = date
                                  BuyPrice = price
                                  Volume = volume } ] }
                    |> fun np -> BuyResult.Success(np, newId)
                | false ->
                    let liquidityVolume = p.Liquidity / price
                    let newInvestment = (volume - liquidityVolume) * price
                    
                    { p with
                        InitialInvestment = p.InitialInvestment + newInvestment
                        Liquidity = 0m // TODO check
                        OpenPositions =
                            p.OpenPositions
                            @ [ { Id = newId
                                  ParentId = None
                                  Symbol = symbol
                                  Start = date
                                  BuyPrice = price
                                  Volume = volume } ] }
                    |> fun np -> BuyResult.Success(np, newId)
            | PrioritizeNewInvestment // Essentially the same as NewInvestmentOnly in this case.
            | NewInvestmentOnly ->
                let newId = System.Guid.NewGuid().ToString("n")

                { p with
                    InitialInvestment = p.InitialInvestment + totalCost
                    OpenPositions =
                        p.OpenPositions
                        @ [ { Id = newId
                              ParentId = None
                              Symbol = symbol
                              Start = date
                              BuyPrice = price
                              Volume = volume } ] }
                |> fun np -> BuyResult.Success(np, newId)
            | LiquidityLimitAsMaximum ->
                // Update to the amount of liquidity.
                let newId = System.Guid.NewGuid().ToString("n")

                let liquidityVolume = p.Liquidity / price
                
                { p with
                    Liquidity = 0m // TODO check
                    OpenPositions =
                        p.OpenPositions
                        @ [ { Id = newId
                              ParentId = None
                              Symbol = symbol
                              Start = date
                              BuyPrice = price
                              Volume = liquidityVolume } ] }
                |> fun np -> BuyResult.Success(np, newId)
            
            

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

                    let newId = System.Guid.NewGuid().ToString()

                    // If volume is less then total volume create an open and closed position.
                    let nop =
                        ({ Id = newId
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
                        Liquidity = p.Liquidity + (volume * price)
                        OpenPositions = oop @ [ nop ]
                        ClosedPositions = p.ClosedPositions @ [ ncp ] })
                    |> fun np -> SellResult.Success(np, Some newId)
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
                    |> fun np -> SellResult.Success(np, None)
            | None -> SellResult.Failure "Position not found"

        member p.GetClosedPositionsForSymbol(symbol) =
            p.ClosedPositions |> List.filter (fun cp -> cp.Symbol = symbol)

    and [<RequireQualifiedAccess>] BuyResult =
        | Success of NewPortfolio: Portfolio * NewId: string
        | Failure of Message: string

    and [<RequireQualifiedAccess>] SellResult =
        | Success of NewPortfolio: Portfolio * NewId: string option
        | Failure of Message: string

    and PositionCondition =
        | PercentageGrowth of Percent: decimal * ValueMapper: ConditionValueMapper
        | FixedValue of Value: decimal * ValueMapper: ConditionValueMapper
        | PercentageLoss of Percent: decimal * ValueMapper: ConditionValueMapper
        | FixedLoss of Value: decimal * ValueMapper: ConditionValueMapper
        | Duration of Length: int
        | Bespoke of Handler: (OpenPosition -> CurrentPosition -> HistoricPositionHandler -> Portfolio -> bool)
        | And of PositionCondition * PositionCondition
        | Or of PositionCondition * PositionCondition
        | All of PositionCondition list
        | Any of PositionCondition list
        | Not of PositionCondition

    and BespokeConditionParameters =
        { Position: OpenPosition
          Current: CurrentPosition
          Portfolio: Portfolio
          HistoricPositionHandler: HistoricPositionHandler }

    and ConditionValueMapper =
        | Value of OHLC: OHLCValue
        | FixedAdjustment of OHLC: OHLCValue * Adjustment: decimal
        | PercentageIncrease of OHLC: OHLCValue * Percentage: decimal
        | PercentageDecrease of OHLC: OHLCValue * Percentage: decimal

        member cvm.GetValue(position: CurrentPosition) =

            match cvm with
            | Value mode -> position.GetValue mode
            | FixedAdjustment(mode, adjustment) -> position.GetValue mode + adjustment
            | PercentageIncrease(mode, percentage) ->
                let bv = position.GetValue mode

                bv + ((bv / 100m) * percentage)
            | PercentageDecrease(mode, percentage) ->
                let bv = position.GetValue mode

                bv - ((bv / 100m) * percentage)

    and [<RequireQualifiedAccess>] PositionAction =
        | IncreasePositionByFixedAmount of Amount: decimal * InvestmentType: InvestmentType
        | IncreasePositionByPercentage of Percent: decimal * InvestmentType: InvestmentType
        | DecreasePositionByFixedAmount of Amount: decimal
        | DecreasePositionByPercentage of Percent: decimal

    and InvestmentType =
        | PrioritizeLiquidity
        | PrioritizeNewInvestment
        | LiquidityOnly
        | NewInvestmentOnly
        | LiquidityLimitAsMaximum

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
        { OHLCValue: OHLCValue 
          ActionCombinationMode: ActionCombinationMode }

    and [<RequireQualifiedAccess>] ActionCombinationMode =
        | Simple
        | Bespoke of Handler: (PrioritizedActions list -> PositionAction list)

    and TriggeredActions =
        { Position: OpenPosition
          Actions: PositionAction list
          CurrentPosition: CurrentPosition }