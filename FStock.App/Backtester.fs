namespace FStock.App

open Freql.Sqlite

module Backtester =

    module V1 =

        open FStock.Modeling.V1
        open FStock.Modeling.V1.Backtesting

        let run (storePath: string) =
            
            use context = SqliteContext()

            let testingCtx =
                ({ Settings =
                    { BuyValueMode = ValueMode.Open
                      SellValueMode = ValueMode.High
                      ActionCombinationMode = ActionCombinationMode.Simple }
                   CurrentPositionHandler = failwith "todo"
                   HistoricPositionsHandler = fun _ -> []
                   NewStateRewriters = [] }
                : TestingContext)

            ()
