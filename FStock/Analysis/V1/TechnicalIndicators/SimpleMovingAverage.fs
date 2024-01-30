namespace FStock.Analysis.V1.TechnicalIndicators

open FStock.Analysis.V1.Core
open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module SimpleMovingAverage =
    
    open System
    open FStock.Analysis.V1.Core

    type Parameters = { WindowSize: int }

    [<CLIMutable>]
    type SmaItem =
        {
            Symbol: string
            EntryDate: DateTime
            Value: decimal
            Sma: decimal
            /// <summary>
            /// Specifies if the value can be discarded.
            /// This normally means the item predates the first calculable entry and thus has no SMA value.
            /// It also means the entry might not be used for further analysis.
            /// </summary>
            Discardable: bool
        }

    type CalculationState =
        { I: int
          Items: SmaItem list }

        static member Empty() = { I = 0; Items = [] }

    let calculate (parameters: Parameters) (values: BasicInputItem list) =
        values
        |> List.fold
            (fun (state: CalculationState) v ->
                let newItem =
                    match state.I with
                    | i when i < (parameters.WindowSize - 1) ->
                        // If less than window size - 1 there is no EMA to be calculated.
                        { Symbol = v.Symbol
                          EntryDate = v.Date
                          Value = v.Price
                          Sma = 0m
                          Discardable = true }
                    | _ ->
                        // If equal to window size - 1 use SMA.
                        { Symbol = v.Symbol
                          EntryDate = v.Date
                          Value = v.Price
                          Sma =
                            state.Items
                            |> List.take (parameters.WindowSize - 1)
                            |> List.map (fun i -> i.Value)
                            |> prepend v.Price 
                            |> List.average
                          Discardable = false }

                { state with
                    I = state.I + 1
                    Items = newItem :: state.Items })
            (CalculationState.Empty())

    let generate (parameters: Parameters) (values: BasicInputItem list) =
        calculate parameters values |> fun r -> r.Items