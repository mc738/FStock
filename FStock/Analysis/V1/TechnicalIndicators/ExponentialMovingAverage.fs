namespace FStock.Analysis.V1.TechnicalIndicators

open System

module ExponentialMovingAverage =

    type Parameters = { WindowSize: int; Smoothing: decimal }

    type InputItem = { Date: DateTime; Price: decimal }

    type EmaItem =
        { Date: DateTime
          Value: decimal
          Ema: decimal }

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
                          Ema = 0m }
                    | i when i = (parameters.WindowSize - 1) ->
                        // If equal to window size - 1 use SMA.
                        { Date = v.Date
                          Value = v.Price
                          Ema = state.Items |> List.averageBy (fun i -> i.Value) }
                    | _ ->
                        { Date = v.Date
                          Value = v.Price
                          Ema =
                            (v.Price * (parameters.Smoothing / decimal (1 + parameters.WindowSize)))
                            + state.Items.Head.Ema
                              * (1m - (parameters.Smoothing / decimal (1 + parameters.WindowSize))) }

                { state with
                    I = state.I + 1
                    Items = newItem :: state.Items })
            (CalculationState.Empty())
