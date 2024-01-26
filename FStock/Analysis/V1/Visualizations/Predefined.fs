namespace FStock.Analysis.V1.Visualizations

open FSVG
open FSVG.Charts
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

    let axisStyle =
        { Fill = None
          Stroke = Some "black"
          StrokeWidth = Some 0.1
          StrokeLineCap = None
          StrokeDashArray = None
          Opacity = Some 1.
          GenericValues = Map.empty }

    let normalizeXValue = ()

    let normalizeYValue
        (value: decimal)
        (minValue: decimal)
        (maxValue: decimal)
        (minPos: float)
        (maxPos: float)
        (flipValue: bool)
        =
        let y =
            ({ MaxValue = maxValue
               MinValue = minValue
               Value = value }
            : NormalizerParameters<decimal>)
            |> rangeNormalizer<decimal> float

        match flipValue with
        | true -> minPos + (((100. - y) / 100.) * (maxPos - minPos))
        | false -> minPos + ((y / 100.) * (maxPos - minPos))

    let createBar
        (value: decimal)
        (minValue: decimal)
        (maxValue: decimal)
        (minYPos: float)
        (maxYPos: float)
        (xPos: float)
        (width: float)
        (flipValue: bool)
        (style: Style)
        =
        // First normalize the value.
        let nv =
            ({ MaxValue = maxValue
               MinValue = minValue
               Value = value }
            : NormalizerParameters<decimal>)
            |> rangeNormalizer<decimal> float

        let actualHeight = maxYPos - minYPos

        let (height, y) =
            let h = (nv / 100.) * actualHeight

            match flipValue with
            | true -> h, minYPos + actualHeight - h
            | false -> h, minYPos


        Rect
            { Height = height
              Width = width
              X = xPos
              Y = y
              RX = 0
              RY = 0
              Style = style }

    let generatePriceChart (settings: Settings) =

        let testStyle =
            { Style.Default() with
                Opacity = Some 1.
                Stroke = Some "green"
                StrokeWidth = Some 0.1 
                Fill = Some "green" }


        [ // First create the axis
          Line
              { X1 = settings.LeftPadding
                X2 = settings.LeftPadding
                Y1 = settings.TopPadding
                Y2 = settings.PriceChartHeight + settings.TopPadding
                Style = axisStyle }
          Line
              { X1 = settings.LeftPadding + settings.Width
                X2 = settings.LeftPadding + settings.Width
                Y1 = settings.TopPadding
                Y2 = settings.PriceChartHeight + settings.TopPadding
                Style = axisStyle }
          Line
              { X1 = (settings.LeftPadding / 2.)
                X2 = settings.LeftPadding + settings.Width + (settings.RightPadding / 2.)
                Y1 = settings.PriceChartHeight + settings.TopPadding
                Y2 = settings.PriceChartHeight + settings.TopPadding
                Style = axisStyle }

          createBar
              25m
              0m
              100m
              settings.TopPadding
              (settings.PriceChartHeight + settings.TopPadding)
              (settings.LeftPadding)
              10.
              true
              testStyle
          
          createBar
              50m
              0m
              100m
              settings.TopPadding
              (settings.PriceChartHeight + settings.TopPadding)
              (settings.LeftPadding + 10.)
              10.
              true
              testStyle
          createBar
              75m
              0m
              100m
              settings.TopPadding
              (settings.PriceChartHeight + settings.TopPadding)
              (settings.LeftPadding + 20.)
              10.
              true
              testStyle
          
          createBar
              25m
              0m
              100m
              settings.TopPadding
              (settings.PriceChartHeight + settings.TopPadding)
              (settings.LeftPadding + 50.)
              10.
              false
              testStyle
          
          createBar
              50m
              0m
              100m
              settings.TopPadding
              (settings.PriceChartHeight + settings.TopPadding)
              (settings.LeftPadding + 60.)
              10.
              false
              testStyle
          createBar
              75m
              0m
              100m
              settings.TopPadding
              (settings.PriceChartHeight + settings.TopPadding)
              (settings.LeftPadding + 70.)
              10.
              false
              testStyle ]

    let generateVolumeChart (settings: Settings) =
        let topOffset = settings.TopPadding + settings.PriceChartHeight


        [ // First create the axis
          Line
              { X1 = settings.LeftPadding
                X2 = settings.LeftPadding
                Y1 = topOffset
                Y2 = topOffset + settings.VolumeHeight
                Style = axisStyle }
          Line
              { X1 = settings.LeftPadding + settings.Width
                X2 = settings.LeftPadding + settings.Width
                Y1 = topOffset
                Y2 = topOffset + settings.VolumeHeight
                Style = axisStyle }
          Line
              { X1 = (settings.LeftPadding / 2.)
                X2 = settings.LeftPadding + settings.Width + (settings.RightPadding / 2.)
                Y1 = topOffset + settings.VolumeHeight
                Y2 = topOffset + settings.VolumeHeight
                Style = axisStyle }

          // Test
          Line
              { X1 = settings.LeftPadding
                X2 = settings.LeftPadding + settings.Width
                Y1 = normalizeYValue 25m 0m 100m topOffset (topOffset + settings.VolumeHeight) true
                Y2 = normalizeYValue 75m 0m 100m topOffset (topOffset + settings.VolumeHeight) true
                Style = axisStyle } ]

    let generateMacdChart (settings: Settings) =
        let topOffset =
            settings.TopPadding + settings.PriceChartHeight + settings.VolumeHeight

        [ // First create the axis
          Line
              { X1 = settings.LeftPadding
                X2 = settings.LeftPadding
                Y1 = topOffset
                Y2 = topOffset + settings.MacdHeight
                Style = axisStyle }
          Line
              { X1 = settings.LeftPadding + settings.Width
                X2 = settings.LeftPadding + settings.Width
                Y1 = topOffset
                Y2 = topOffset + settings.MacdHeight
                Style = axisStyle }
          Line
              { X1 = (settings.LeftPadding / 2.)
                X2 = settings.LeftPadding + settings.Width + (settings.RightPadding / 2.)
                Y1 = topOffset + settings.MacdHeight
                Y2 = topOffset + settings.MacdHeight
                Style = axisStyle } ]

    let generateRsiChart (settings: Settings) =
        let topOffset =
            settings.TopPadding
            + settings.PriceChartHeight
            + settings.VolumeHeight
            + settings.RsiHeight

        [ // First create the axis
          Line
              { X1 = settings.LeftPadding
                X2 = settings.LeftPadding
                Y1 = topOffset
                Y2 = topOffset + settings.RsiHeight
                Style = axisStyle }
          Line
              { X1 = settings.LeftPadding + settings.Width
                X2 = settings.LeftPadding + settings.Width
                Y1 = topOffset
                Y2 = topOffset + settings.RsiHeight
                Style = axisStyle }
          Line
              { X1 = (settings.LeftPadding / 2.)
                X2 = settings.LeftPadding + settings.Width + (settings.RightPadding / 2.)
                Y1 = topOffset + settings.RsiHeight
                Y2 = topOffset + settings.RsiHeight
                Style = axisStyle }

          Line
              { X1 = settings.LeftPadding
                X2 = settings.LeftPadding + settings.Width
                Y1 = normalizeYValue 30m 0m 100m topOffset (topOffset + settings.RsiHeight) true
                Y2 = normalizeYValue 30m 0m 100m topOffset (topOffset + settings.RsiHeight) true
                Style =
                  { axisStyle with
                      Stroke = Some "blue"
                      StrokeWidth = Some 0.5
                      StrokeLineCap = Some StrokeLineCap.Butt
                      StrokeDashArray = Some [ 2; 2 ]
                      GenericValues = [ "stroke-dashoffset", "1" ] |> Map.ofList } }

          Line
              { X1 = settings.LeftPadding
                X2 = settings.LeftPadding + settings.Width
                Y1 = normalizeYValue 70m 0m 100m topOffset (topOffset + settings.RsiHeight) true
                Y2 = normalizeYValue 70m 0m 100m topOffset (topOffset + settings.RsiHeight) true
                Style =
                  { axisStyle with
                      Stroke = Some "blue"
                      StrokeWidth = Some 0.5
                      StrokeLineCap = Some StrokeLineCap.Butt
                      StrokeDashArray = Some [ 2; 2 ]
                      GenericValues = [ "stroke-dashoffset", "1" ] |> Map.ofList } }

          Rect
              { Height = normalizeYValue 40m 0m 100m 0 (settings.RsiHeight) false
                Width = settings.Width
                X = settings.LeftPadding
                Y = normalizeYValue 30m 0m 100m topOffset (topOffset + settings.RsiHeight) false
                RX = 0.
                RY = 0.
                Style =
                  { Fill = Some "blue"
                    Stroke = None
                    StrokeWidth = None
                    StrokeLineCap = None
                    StrokeDashArray = None
                    Opacity = Some 0.2
                    GenericValues = Map.empty } } ]

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
              VolumeHeight = 20.
              MacdHeight = 40.
              RsiHeight = 40.
              Width = 180. }

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
