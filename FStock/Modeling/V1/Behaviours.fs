﻿namespace FStock.Modeling.V1

[<RequireQualifiedAccess>]
module Behaviours =

    module Optimistic =

        /// <summary>
        /// If a position is up 10% reinvest 10% more in it.
        /// </summary>
        let ``matched 10% reinvestment in winning position`` =
            ({ Condition = Conditions.``percentage take profit based on high value`` 10m
               Actions = [ PositionAction.IncreasePositionByPercentage(10m, InvestmentType.LiquidityOnly) ] }
            : PositionBehaviour)
        
        let ``take profit at 20%`` =
            ({ Condition = Conditions.``percentage take profit based on high value`` 20m
               Actions = [ PositionAction.DecreasePositionByFixedAmount 100m ] })

        let all = [ ``matched 10% reinvestment in winning position`` ]

    module Pessimistic =
        
        
        let ``10% stop-loss`` =
            ({ Condition = Conditions.``percentage stop-loss based on low value`` 10m
               Actions = [ PositionAction.DecreasePositionByFixedAmount 100m ] })

        let all = []

    module LowRiskTolerance =
        let all = []

    module HighRiskTolerance =
        let all = []

    
    
    let all =
        [ yield! Optimistic.all
          yield! Pessimistic.all
          yield! LowRiskTolerance.all
          yield! HighRiskTolerance.all ]
