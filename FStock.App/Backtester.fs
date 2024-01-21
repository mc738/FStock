namespace FStock.App

open System
open FStock.Data.Store
open FStock.Modeling.V1
open Freql.Sqlite

module Backtester =

    module V1 =

        open FStock.Modeling.V1

        module Behaviours =

            module Optimistic =

                /// <summary>
                /// If a position is up 10% reinvest 10% more in it.
                /// </summary>
                let ``matched 10% reinvestment in winning position`` =
                    ({ Condition = Conditions.``percentage take profit`` 10m
                       Actions =
                         [ PositionAction.IncreasePositionByPercentage(10m, InvestmentType.LiquidityOnly) ] }
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

        let advance (ctx: Backtesting.TestingContext) (date: DateTime) (state: Backtesting.ModelState) =

            Backtesting.progressState ctx date state

        let run (storePath: string) (startDate: DateTime) (initialState: Backtesting.ModelState) =

            use store = FStockStore.Open storePath

            let testingCtx =
                ({ Settings =
                    { BuyValueMode = ValueMode.Open
                      SellValueMode = ValueMode.High
                      ActionCombinationMode = ActionCombinationMode.Simple }
                   CurrentPositionHandler = Data.getCurrentPosition store
                   HistoricPositionsHandler = Data.getHistoricPositions store
                   NewStateRewriters = [] }
                : Backtesting.TestingContext)

            // Create initial state

            let rec handler (date: DateTime, state: Backtesting.ModelState) =
                match advance testingCtx date state with
                | Backtesting.NewStateResult.Changed(ns, log) ->
                    // Add extra checks


                    handler (date.AddDays(1), ns)

            handler (startDate, initialState)

            ()
