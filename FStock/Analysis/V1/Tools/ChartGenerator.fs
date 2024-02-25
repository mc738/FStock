namespace FStock.Analysis.V1.Tools

open FStock.Analysis.V1.Visualizations.Charts
open Microsoft.FSharp.Core

module ChartGenerator =

    [<RequireQualifiedAccess>]
    type ChartType =
        | Price of PriceChartSettings
        | Volume of VolumeChartSettings
        | Macd of MacdChartSettings
        | Rsi of RsiChartSettings

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
