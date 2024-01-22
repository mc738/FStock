namespace FStock.Modeling.V1

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

        let all = [ ``matched 10% reinvestment in winning position`` ]

    module Pessimistic =

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
