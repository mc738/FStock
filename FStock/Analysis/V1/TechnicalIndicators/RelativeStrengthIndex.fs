namespace FStock.Analysis.V1.TechnicalIndicators

open System
open FStock.Analysis.V1.Persistence

[<RequireQualifiedAccess>]
module RelativeStrengthIndex =

    type Parameters =
        { Periods: int }

        static member Default() = { Periods = 14 }

    type InputItem = { Date: DateTime; Price: decimal }

    type RsiItem =
        { Date: DateTime
          Close: decimal
          Gain: decimal
          Loss: decimal
          AverageGain: decimal
          AverageLoss: decimal
          Rsi: decimal }

    type CalculationState =
        { I: int
          Items: RsiItem list
          Gains: decimal list
          Losses: decimal list }

        static member Empty() = { Items = []; Gains = []; Losses = [] }

    let calculate (parameters: Parameters) (values: InputItem list) =
        values
        |> List.fold
            (fun (state: CalculationState) v ->
                match state.Items.IsEmpty with
                | true ->
                    let item =
                        { Date = v.Date
                          Close = v.Price
                          Gain = 0m
                          Loss = 0m
                          AverageGain = 0m
                          AverageLoss = 0m
                          Rsi = 0m }

                    { state with
                        Items = item :: state.Items
                        I = state.I + 1 }

                //(Some item, item :: acc)
                | false ->
                    // Because the items are currently in reverse order we can just grab the head to get the last one,
                    let prevItem = state.Items.Head



                    let difference = v.Price - prevItem.Close


                    let (newGains, newLosses) =
                        match difference with
                        | d when d > 0m -> difference :: state.Gains, 0m :: state.Losses
                        | d when d < 0m -> 0m :: state.Gains, difference :: state.Losses
                        | _ -> 0m :: state.Gains, 0m :: state.Losses

                    match state.I with
                    | i when i < parameters.Periods ->
                        {
                            
                            
                        }
                    | i when i = parameters.Periods -> ()
                    | _ -> ()



                    { state })
            (CalculationState.Empty(), 0)

    let calculate _ =

        let averageGain = 0m
        let averageLoss = 0m




        ()



    ()
