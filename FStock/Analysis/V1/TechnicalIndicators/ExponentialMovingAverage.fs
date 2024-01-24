namespace FStock.Analysis.V1.TechnicalIndicators

open System

module ExponentialMovingAverage =

    type Parameters = { WindowSize: int; Smoothing: int }

    type InputItem = { Date: DateTime; Price: decimal }
    
    type EmaItem =
        { Date: DateTime
          Value: decimal
          Ema: decimal }

    type CalculationState =
        { I: int
          Items: EmaItem list }
    
    let calculate =
        ()