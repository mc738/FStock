namespace FStock.App

open System
open FStock.Data.Store
open FStock.Modeling.V1
open Freql.Sqlite

module Backtester =

    module V1 =

        open FStock.Modeling.V1

        let advance (ctx: Backtesting.TestingContext) (date: DateTime) (state: Backtesting.ModelState) =

            Backtesting.progressState ctx date state

        let run
            (storePath: string)
            (startDate: DateTime)
            (initialState: Backtesting.ModelState)
            (progressionCallbackFn: Backtesting.ModelState -> string list -> bool)
            =

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
                    match progressionCallbackFn ns log with
                    | true -> handler (date.AddDays(1), ns)
                    | false -> ns

            handler (startDate, initialState)
