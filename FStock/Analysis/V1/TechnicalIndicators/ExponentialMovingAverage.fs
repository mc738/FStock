namespace FStock.Analysis.V1.TechnicalIndicators

open System

[<RequireQualifiedAccess>]
module ExponentialMovingAverage =

    type Parameters = { WindowSize: int; Smoothing: decimal }

    type InputItem = { Date: DateTime; Price: decimal }

    type EmaItem =
        { Date: DateTime
          Value: decimal
          Ema: decimal
          /// <summary>
          /// Specifies if the value can be discarded.
          /// This normally means the item predates the first calculable entry and thus has no EMA value.
          /// It also means the entry might not be used for further analysis. 
          /// </summary>
          Discardable: bool }

    type CalculationState =
        { I: int
          Items: EmaItem list }

        static member Empty() = { I = 0; Items = [] }

    let calculate (parameters: Parameters) (values: InputItem list) =
        values
        |> List.fold
            (fun (state: CalculationState) v ->
                let newItem =
                    match state.I with
                    | i when i < (parameters.WindowSize - 1) ->
                        // If less than window size - 1 there is no EMA to be calculated.
                        { Date = v.Date
                          Value = v.Price
                          Ema = 0m
                          Discardable = true }
                    | i when i = (parameters.WindowSize - 1) ->
                        // If equal to window size - 1 use SMA.
                        { Date = v.Date
                          Value = v.Price
                          Ema = state.Items |> List.map (fun i -> i.Value) |> fun l -> v.Price :: l |> List.average
                          Discardable = false }
                    | _ ->
                        { Date = v.Date
                          Value = v.Price
                          Ema =
                            (v.Price * (parameters.Smoothing / decimal (1 + parameters.WindowSize)))
                            + state.Items.Head.Ema
                              * (1m - (parameters.Smoothing / decimal (1 + parameters.WindowSize)))
                          Discardable = false }

                { state with
                    I = state.I + 1
                    Items = newItem :: state.Items })
            (CalculationState.Empty())
