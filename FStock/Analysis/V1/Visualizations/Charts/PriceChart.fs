namespace FStock.Analysis.V1.Visualizations.Charts

open FSVG.Charts

[<RequireQualifiedAccess>]
module PriceChart =

    open FSVG

    let testStyle =
        { Style.Default() with
            Opacity = Some 1.
            Stroke = Some "green"
            StrokeWidth = Some 0.1
            Fill = Some "green" }

    type Parameters =
        { MinimumX: float
          MaximumX: float
          MinimumY: float
          MaximumY: float
          LeftYAxis: bool
          RightYAxis: bool
          XAxisStartOverride: float option
          XAxisEndOverride: float option
          AxisStyle: Style
          Data: StockData }

    let createLabels (parameters: Parameters) (minValue: decimal) (maxValue: decimal) =

        let diff = maxValue - minValue

        [ 1m; 2m; 3m; 4m; 5m ]
        |> List.collect (fun i ->
            [ Text
                  { X = 1.
                    Y = normalizeYValue (i * 20m) 0m 100m parameters.MinimumY parameters.MaximumY true
                    Value = [ TextType.Literal((minValue + ((diff / 5m) * i)).ToString("#.##")) ]
                    Style =
                      { Style.Default() with
                          Opacity = Some 1.
                          Fill = Some "black"
                          GenericValues =
                              [ "font-size", "4px"
                                "alignment-baseline",
                                "central" (*"text-anchor", "middle"; "font-family", "\"roboto\""*) ]
                              |> Map.ofList } }

              Text
                  { X = (parameters.MaximumX + 2.)
                    Y = normalizeYValue (i * 20m) 0m 100m parameters.MinimumY parameters.MaximumY true
                    Value = [ TextType.Literal((minValue + ((diff / 5m) * i)).ToString("#.##")) ]
                    Style =
                      { Style.Default() with
                          Opacity = Some 1.
                          Fill = Some "black"
                          GenericValues =
                              [ "font-size", "4px"
                                "alignment-baseline",
                                "central" (*"text-anchor", "middle"; "font-family", "\"roboto\""*) ]
                              |> Map.ofList } }

              ])

    let createCandleSticks (parameters: Parameters) (minValue: decimal) (maxValue: decimal) =

        let sectionPadding = 0.5

        let sectionWidth =
            (parameters.MaximumX - parameters.MinimumX)
            / float parameters.Data.BaseData.Length

        let barWidth = sectionWidth - (sectionPadding * 2.)

        parameters.Data.BaseData
        |> List.mapi (fun i v ->
            let normalizeValue (value: decimal) =
                ({ MaxValue = maxValue
                   MinValue = minValue
                   Value = value }
                : NormalizerParameters<decimal>)
                |> rangeNormalizer<decimal> float

            let top, bottom, color =
                // ValueA is CloseValue because this is comparing if the value went up or down over the period.
                match
                    { ValueA = v.CloseValue
                      ValueB = v.OpenValue }
                    |> decimalValueComparer
                with
                | ValueComparisonResult.GreaterThan ->
                    normalizeValue v.CloseValue, normalizeValue v.OpenValue, SvgColor.Named "green" // series.Style.PositiveColor
                | ValueComparisonResult.LessThan ->
                    normalizeValue v.OpenValue, normalizeValue v.CloseValue, SvgColor.Named "red"
                | ValueComparisonResult.Equal ->
                    normalizeValue v.CloseValue, normalizeValue v.OpenValue, SvgColor.Named "green"

            let height = ((parameters.MaximumY - parameters.MinimumY) / 100.) * (top - bottom)

            [ ({ Height = height
                 Width = barWidth
                 X = parameters.MinimumX + sectionPadding + (float i * sectionWidth)
                 Y =
                   parameters.MinimumY
                   + (((100. - bottom - (top - bottom)) / 100.)
                      * (parameters.MaximumY - parameters.MinimumY))
                 RX = 0.
                 RY = 0.
                 Style =
                   { Fill = color.GetValue() |> Some
                     Stroke = None
                     StrokeWidth = None
                     StrokeLineCap = None
                     StrokeDashArray = None
                     Opacity = Some 1.
                     GenericValues = Map.empty } }
              : RectElement)
              |> Element.Rect

              ({ X1 = parameters.MinimumX + (float i * sectionWidth) + (sectionWidth / 2.)
                 X2 = parameters.MinimumX + (float i * sectionWidth) + (sectionWidth / 2.)
                 Y1 =
                   parameters.MinimumY
                   + ((100. - normalizeValue v.HighValue) / 100.)
                     * (parameters.MaximumY - parameters.MinimumY)
                 Y2 =
                   parameters.MinimumY
                   + ((100. - normalizeValue v.LowValue) / 100.)
                     * (parameters.MaximumY - parameters.MinimumY)
                 Style =
                   { Fill = None
                     Stroke = color.GetValue() |> Some
                     StrokeWidth = Some 0.5
                     StrokeLineCap = None
                     StrokeDashArray = None
                     Opacity = Some 1.
                     GenericValues = Map.empty } }
              : LineElement)
              |> Element.Line ])
        |> List.concat

    let createCurrentLine (parameters: Parameters) (minValue: decimal) (maxValue: decimal) =

        let item = parameters.Data.BaseData |> List.last

        let y =
            normalizeYValue (item.CloseValue) minValue maxValue parameters.MinimumY parameters.MaximumY true

        [ Line
              { X1 = parameters.MinimumX
                X2 = parameters.MaximumX
                Y1 = y
                Y2 = y
                Style =
                  { parameters.AxisStyle with
                      StrokeDashArray = Some [ 1; 1 ]
                      GenericValues = [ "stroke-dashoffset", "1" ] |> Map.ofList } } ]

    let create (parameters: Parameters) =

        let baseMaxValue =
            parameters.Data.BaseData
            |> List.maxBy (fun e -> e.HighValue)
            |> fun e -> e.HighValue

        let baseMinValue =
            parameters.Data.BaseData
            |> List.minBy (fun e -> e.LowValue)
            |> fun e -> e.LowValue

        let (minValue, maxValue) = createMinMaxValues baseMinValue baseMaxValue


        [ // First create the axis
          (*
          Text
              { X = 1.
                Y = parameters.MinimumY + 5.
                Value = [ TextType.Literal "Price" ]
                Style =
                  { Style.Default() with
                      Opacity = Some 1.
                      Fill = Some "black"
                      GenericValues =
                          [ "font-size", "4px" (*"text-anchor", "middle"; "font-family", "\"roboto\""*) ]
                          |> Map.ofList } }
          *)

          Line
              { X1 = parameters.MinimumX
                X2 = parameters.MinimumX
                Y1 = parameters.MinimumY
                Y2 = parameters.MaximumY
                Style = parameters.AxisStyle }
          Line
              { X1 = parameters.MaximumX
                X2 = parameters.MaximumX
                Y1 = parameters.MinimumY
                Y2 = parameters.MaximumY
                Style = parameters.AxisStyle }
          Line
              { X1 = parameters.XAxisStartOverride |> Option.defaultValue parameters.MinimumX
                X2 = parameters.XAxisEndOverride |> Option.defaultValue parameters.MaximumX
                Y1 = parameters.MaximumY
                Y2 = parameters.MaximumY
                Style = parameters.AxisStyle }

          yield! createLabels parameters minValue maxValue

          yield! createCurrentLine parameters minValue maxValue
          yield! createCandleSticks parameters minValue maxValue ]
