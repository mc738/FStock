namespace FStock.Analysis.V1.Visualizations

open Microsoft.FSharp.Core

module Predefined =

    open FSVG

    type Settings =
        { LeftPadding: float
          RightPadding: float
          TopPadding: float
          BottomPadding: float
          PriceChartHeight: float
          VolumeHeight: float
          MacdHeight: float
          RsiHeight: float
          Width: float }

    let generatePriceChart (settings: Settings) =
        let axisStyle =
            { Fill = None
              Stroke = None
              StrokeWidth = Some 0.1
              Opacity = Some 1.
              GenericValues = Map.empty }


        [ // First create the axis
          Line
              { X1 = settings.LeftPadding
                X2 = settings.LeftPadding
                Y1 = settings.TopPadding
                Y2 = settings.PriceChartHeight
                Style = axisStyle } ]

    let generateVolumeChart (settings: Settings) = []

    let generateMacdChart (settings: Settings) = []

    let generateRsiChart (settings: Settings) = []

    let generate _ =
        // This is made up of 4 parts:
        // * Price chart (candle stick)
        // * Volume (bar)
        // * Mcad (line + histogram)
        // * Rsi

        let settings =
            { LeftPadding = 10.
              RightPadding = 10.
              TopPadding = 10.
              BottomPadding = 10.
              PriceChartHeight = 80.
              VolumeHeight = 30.
              MacdHeight = 30.
              RsiHeight = 30.
              Width = 80. }

        [ yield! generatePriceChart settings
          yield! generateVolumeChart settings
          yield! generateMacdChart settings
          yield! generateRsiChart settings ]
        |> SvgDocument.Create
        |> fun d ->
            d.Render(
                (settings.TopPadding
                 + settings.PriceChartHeight
                 + settings.VolumeHeight
                 + settings.MacdHeight
                 + settings.RsiHeight
                 + settings.BottomPadding)
                |> int,
                (settings.LeftPadding + settings.Width + settings.RightPadding) |> int
            )


    ()
