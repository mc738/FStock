namespace FStock.Analysis.V1.TechnicalIndicators

open System
open FStock.Analysis.V1.Core

[<RequireQualifiedAccess>]
module MovingAverageConvergenceDivergence =

    type Parameters =
        { LongTermPeriods: int
          LongTermEmaSmoothing: decimal
          ShortTermPeriods: int
          ShortTermEmaSmoothing: decimal
          SignalPeriods: int
          SignalEmaSmoothing: decimal }

        static member Default() =
            { LongTermPeriods = 26
              LongTermEmaSmoothing = 2m
              ShortTermPeriods = 12
              ShortTermEmaSmoothing = 2m
              SignalPeriods = 9
              SignalEmaSmoothing = 2m }

    type MacdItem =
        {
            Date: DateTime
            LongTermEma: decimal
            ShortTermEma: decimal
            SignalEma: decimal
            MacdValue: decimal
            /// <summary>
            /// Specifies if the value can be discarded.
            /// This normally means the item predates the first calculable entry and thus has no EMA value.
            /// It also means the entry might not be used for further analysis.
            /// </summary>
            Discardable: bool
        }


    let generate (parameters: Parameters) (values: BasicInputItem list) =
        // NOTE currently this over calculates, there is no need to calculate short term or signal values that will be discarded.
        // However it should also be relatively cheap so will do for now.

        let longTermEma =
            ExponentialMovingAverage.generate
                ({ WindowSize = parameters.LongTermPeriods
                   Smoothing = parameters.LongTermEmaSmoothing }
                : ExponentialMovingAverage.Parameters)
                values

        let shortTermEma =
            ExponentialMovingAverage.generate
                ({ WindowSize = parameters.ShortTermPeriods
                   Smoothing = parameters.ShortTermEmaSmoothing }
                : ExponentialMovingAverage.Parameters)
                values

        let signalEma =
            ExponentialMovingAverage.generate
                ({ WindowSize = parameters.SignalPeriods
                   Smoothing = parameters.SignalEmaSmoothing }
                : ExponentialMovingAverage.Parameters)
                values

        List.zip3 longTermEma shortTermEma signalEma
        |> List.map (fun (lt, st, s) ->
            { Date = lt.Date
              LongTermEma = lt.Ema
              ShortTermEma = st.Ema
              SignalEma = s.Ema
              MacdValue = st.Ema - lt.Ema
              Discardable = lt.Discardable || st.Discardable || s.Discardable })
