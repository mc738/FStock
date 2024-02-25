namespace FStock.Analysis.V1.Tools

open Microsoft.FSharp.Core

module ChartGenerator =

    [<RequireQualifiedAccess>]
    type ChartType =
        | Price
        | Volume 
        | Macd
        | Rsi

    and PriceChartSettings = { Height: float }

    and VolumeChartSettings = { Height: float }
    
    and MacdChartSettings = { Height: float }
    
    and RsiChartSettings = { Height: float }

    and GeneralSettings =
        { Width: float
          LeftPadding: float
          RightPadding: float
          TopPadding: float
          BottomPadding: float }


    ()
