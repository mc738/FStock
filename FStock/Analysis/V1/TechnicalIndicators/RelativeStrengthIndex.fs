namespace FStock.Analysis.V1.TechnicalIndicators

open System
open System.Collections.Generic
open FStock.Analysis.V1.Core
open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module RelativeStrengthIndex =

    type Parameters =
        { Periods: int
          RoundHandler: decimal -> decimal }

        static member Default() = {
            Periods = 14
            RoundHandler = id 
        }

    [<CLIMutable>]
    type RsiItem =
        { Symbol: string
          EntryDate: DateTime
          CloseValue: decimal
          GainValue: decimal
          LossValue: decimal
          AverageGain: decimal
          AverageLoss: decimal
          Rsi: decimal
          /// <summary>
          /// Specifies if the value can be discarded.
          /// This normally means the item predates the first calculable entry and thus has no RSI value.
          /// It also means the entry might not be used for further analysis. 
          /// </summary>
          Discardable: bool }

    type CalculationState =
        { I: int
          Items: RsiItem list }

        static member Empty() = { I = 0; Items = [] }

    let calculate (parameters: Parameters) (values: BasicInputItem list) =
        // Influenced by https://www.alpharithms.com/relative-strength-index-rsi-in-python-470209/
        
        let gains = Queue<decimal>()
        let losses = Queue<decimal>()

        values
        |> List.fold
            (fun (state: CalculationState) v ->
                match state.Items.IsEmpty with
                | true ->
                    let item =
                        { Symbol = v.Symbol
                          EntryDate = v.Date
                          CloseValue = v.Price
                          GainValue = 0m
                          LossValue = 0m
                          AverageGain = 0m
                          AverageLoss = 0m
                          Rsi = 0m
                          Discardable = true }

                    { state with
                        Items = item :: state.Items
                        I = state.I + 1 }

                //(Some item, item :: acc)
                | false ->
                    // Because the items are currently in reverse order we can just grab the head to get the last one,
                    let prevItem = state.Items.Head

                    let difference = (v.Price - prevItem.CloseValue) |> parameters.RoundHandler

                    let (gain, loss) =
                        match difference with
                        | d when d > 0m ->
                            gains.Enqueue(d)
                            losses.Enqueue(0)
                            d, 0m

                        | d when d < 0m ->
                            gains.Enqueue(0)
                            losses.Enqueue(abs d)
                            0m, abs d
                        | _ ->
                            gains.Enqueue(0)
                            losses.Enqueue(0)
                            0m, 0m

                    let newItem =
                        match state.I with
                        | i when i < parameters.Periods ->
                            { Symbol = v.Symbol
                              EntryDate = v.Date
                              CloseValue = v.Price
                              GainValue = gain
                              LossValue = loss
                              AverageGain = 0m
                              AverageLoss = 0m
                              Rsi = 0m
                              Discardable = true }
                        | i when i = parameters.Periods ->
                            let avgGain = (Seq.sum gains / (decimal gains.Count)) |> parameters.RoundHandler
                            let avgLoss = (Seq.sum losses / (decimal losses.Count)) |> parameters.RoundHandler

                            let rs = (avgGain / avgLoss) |> parameters.RoundHandler

                            { Symbol = v.Symbol
                              EntryDate = v.Date
                              CloseValue = v.Price
                              GainValue = gain
                              LossValue = loss
                              AverageGain = avgGain
                              AverageLoss = avgLoss
                              Rsi = (100m - (100m / (1m + rs))) |> parameters.RoundHandler
                              Discardable = false }
                        | _ ->
                            let avgGain =
                                ((prevItem.AverageGain * decimal (parameters.Periods - 1) + gain)
                                / decimal parameters.Periods) |> parameters.RoundHandler

                            let avgLoss =
                                ((prevItem.AverageLoss * decimal (parameters.Periods - 1) + loss)
                                / decimal parameters.Periods) |> parameters.RoundHandler

                            let rs = (avgGain / avgLoss) |> parameters.RoundHandler

                            { Symbol = v.Symbol
                              EntryDate = v.Date
                              CloseValue = v.Price
                              GainValue = gain
                              LossValue = loss
                              AverageGain = avgGain
                              AverageLoss = avgLoss
                              Rsi = (100m - (100m / (1m + rs))) |> parameters.RoundHandler
                              Discardable = false }

                    // If i is bigger than periods, progress the window by popping last items in gains and losses.
                    if state.I >= parameters.Periods then
                        gains.Dequeue() |> ignore
                        losses.Dequeue() |> ignore

                    { state with
                        I = state.I + 1
                        Items = newItem :: state.Items })
            (CalculationState.Empty())

    let generate (parameters: Parameters) (values: BasicInputItem list) =
        calculate parameters values |> fun r -> r.Items
