namespace FStock.Analysis.V1

open FStock.Analysis.V1.TechnicalIndicators
open Microsoft.FSharp.Core

module Store =


    type BuildSettings =
        { Sampling: SamplingType
          TechnicalIndicators: TechnicalIndicator list }

    and [<RequireQualifiedAccess>] SamplingType =
        | None
        | Fixed of int
        | Percentage of float

    and TechnicalIndicator =
        | Sma of TableName: string * Parameters: SimpleMovingAverage.Parameters
        | Ema of TableName: string * Parameters: ExponentialMovingAverage.Parameters
        | Rsi of TableName: string * Parameters: RelativeStrengthIndex.Parameters
        | Macd of TableName: string * Parameters: RelativeStrengthIndex.Parameters

    and 
    
    let build (settings: BuildSettings) (path: string) =
        
        ()