namespace FStock.Analysis.V1.Visualizations.Charts

open FSVG.Charts
open FStock.Analysis.V1.Core
open FStock.Analysis.V1.TechnicalIndicators

[<RequireQualifiedAccess>]
module PriceChart =

    open FSVG

    let testStyle =
        { Style.Default() with
            Opacity = Some 1.
            Stroke = Some "green"
            StrokeWidth = Some 0.1
            Fill = Some "green" }

    type ChartSettings =
        { MinimumX: float
          MaximumX: float
          MinimumY: float
          MaximumY: float
          LeftYAxis: bool
          RightYAxis: bool
          XAxisStartOverride: float option
          XAxisEndOverride: float option
          AxisStyle: Style }

    type StockDataParameters =
        { ChartSettings: ChartSettings
          Data: StockData }

    let createLabels (settings: ChartSettings) (minValue: decimal) (maxValue: decimal) =

        let diff = maxValue - minValue

        [ 1m; 2m; 3m; 4m; 5m ]
        |> List.collect (fun i ->
            [ Text
                  { X = 1.
                    Y = normalizeYValue (i * 20m) 0m 100m settings.MinimumY settings.MaximumY true
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
                  { X = (settings.MaximumX + 2.)
                    Y = normalizeYValue (i * 20m) 0m 100m settings.MinimumY settings.MaximumY true
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

              Line
                  { X1 = settings.MinimumX - 0.5
                    X2 = settings.MaximumX + 0.5
                    Y1 = normalizeYValue (i * 20m) 0m 100m settings.MinimumY settings.MaximumY true
                    Y2 = normalizeYValue (i * 20m) 0m 100m settings.MinimumY settings.MaximumY true
                    Style =
                      { settings.AxisStyle with
                          Opacity = Some 0.5 } }

              ])

    let createMovingAverageLines
        (settings: ChartSettings)
        (minValue: decimal)
        (maxValue: decimal)
        (itemCount: int)
        (shortMovingAveragesData: SimpleMovingAverage.SmaItem list)
        (longMovingAveragesData: SimpleMovingAverage.SmaItem list)
        =

        let sectionWidth = (settings.MaximumX - settings.MinimumX) / float itemCount


        [ Path
              { Commands =
                  shortMovingAveragesData
                  |> List.mapi (fun i d ->
                      ({ X = settings.MinimumX + (float i * sectionWidth) + (sectionWidth / 2.)
                         Y = normalizeYValue d.Sma minValue maxValue settings.MinimumY settings.MaximumY true }
                      : SvgPoint))
                  |> SvgPoints.Create
                  |> Helpers.createBezierCommands
                Style =
                  { Style.Default() with
                      Opacity = Some 1
                      StrokeWidth = Some 0.1
                      Stroke = Some "blue" } }

          Path
              { Commands =
                  longMovingAveragesData
                  |> List.mapi (fun i d ->
                      ({ X = settings.MinimumX + (float i * sectionWidth) + (sectionWidth / 2.)
                         Y = normalizeYValue d.Sma minValue maxValue settings.MinimumY settings.MaximumY true }
                      : SvgPoint))
                  |> SvgPoints.Create
                  |> Helpers.createBezierCommands
                Style =
                  { Style.Default() with
                      Opacity = Some 1
                      StrokeWidth = Some 0.1
                      Stroke = Some "orange" } } ]

    let createCandleSticks
        (settings: ChartSettings)
        (itemCount: int)
        (minValue: decimal)
        (maxValue: decimal)
        (baseData: InstrumentPositionEntry list)
        =

        let sectionPadding = 0.5

        let sectionWidth = (settings.MaximumX - settings.MinimumX) / float itemCount

        let barWidth = sectionWidth - (sectionPadding * 2.)

        baseData
        |> List.mapi (fun i v ->
            let normalizeValue (value: decimal) =
                ({ MaxValue = maxValue
                   MinValue = minValue
                   Value = value }
                : NormalizerParameters<decimal>)
                |> rangeNormalizer<decimal> float

            let top, bottom, color =
                // ValueA is CloseValue because this is comparing if the value went up or down over the period.
                match { ValueA = v.Close; ValueB = v.Open } |> decimalValueComparer with
                | ValueComparisonResult.GreaterThan ->
                    normalizeValue v.Close, normalizeValue v.Open, SvgColor.Named "green" // series.Style.PositiveColor
                | ValueComparisonResult.LessThan -> normalizeValue v.Open, normalizeValue v.Close, SvgColor.Named "red"
                | ValueComparisonResult.Equal -> normalizeValue v.Close, normalizeValue v.Open, SvgColor.Named "green"

            let height = ((settings.MaximumY - settings.MinimumY) / 100.) * (top - bottom)

            [ ({ Height = height
                 Width = barWidth
                 X = settings.MinimumX + sectionPadding + (float i * sectionWidth)
                 Y =
                   settings.MinimumY
                   + (((100. - bottom - (top - bottom)) / 100.)
                      * (settings.MaximumY - settings.MinimumY))
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

              ({ X1 = settings.MinimumX + (float i * sectionWidth) + (sectionWidth / 2.)
                 X2 = settings.MinimumX + (float i * sectionWidth) + (sectionWidth / 2.)
                 Y1 =
                   settings.MinimumY
                   + ((100. - normalizeValue v.High) / 100.)
                     * (settings.MaximumY - settings.MinimumY)
                 Y2 =
                   settings.MinimumY
                   + ((100. - normalizeValue v.Low) / 100.) * (settings.MaximumY - settings.MinimumY)
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

    let createCurrentLine
        (settings: ChartSettings)
        (minValue: decimal)
        (maxValue: decimal)
        (baseData: InstrumentPositionEntry list)
        =

        let item = baseData |> List.last

        let y =
            normalizeYValue (item.Close) minValue maxValue settings.MinimumY settings.MaximumY true

        [ Line
              { X1 = settings.MinimumX
                X2 = settings.MaximumX
                Y1 = y
                Y2 = y
                Style =
                  { settings.AxisStyle with
                      StrokeDashArray = Some [ 1; 1 ]
                      GenericValues = [ "stroke-dashoffset", "1" ] |> Map.ofList } } ]

    let create
        (settings: ChartSettings)
        (data: Instrument)
        (shortMoveAverageItems: SimpleMovingAverage.SmaItem list)
        (longMoveAverageItems: SimpleMovingAverage.SmaItem list)
        =

        let baseMaxValue = data.GetMaxValue()

        let baseMinValue = data.GetMinValue()

        (*
        let maItems =
            parameters.Data.All()
            |> List.map (fun d ->
                { Symbol = ""
                  Date = d.EntryDate
                  Price = d.CloseValue }

                : BasicInputItem)
        *)
        (*
        let ma50 =
            SimpleMovingAverage.generate { WindowSize = 50 } maItems
            |> List.take parameters.Data.BaseData.Length
            |> List.rev


        let ma200 =
            SimpleMovingAverage.generate { WindowSize = 200 } maItems
            |> List.take parameters.Data.BaseData.Length
            |> List.rev
        *)

        let (minValue, maxValue) =
            createMinMaxValues
                (List.min
                    [ baseMinValue
                      shortMoveAverageItems |> List.minBy (fun v -> v.Sma) |> (fun r -> r.Sma)
                      longMoveAverageItems |> List.minBy (fun v -> v.Sma) |> (fun r -> r.Sma) ])
                (List.max
                    [ baseMaxValue
                      shortMoveAverageItems |> List.maxBy (fun v -> v.Sma) |> (fun r -> r.Sma)
                      longMoveAverageItems |> List.maxBy (fun v -> v.Sma) |> (fun r -> r.Sma) ])

        [ // First create the axis
          Line
              { X1 = settings.MinimumX
                X2 = settings.MinimumX
                Y1 = settings.MinimumY
                Y2 = settings.MaximumY
                Style = settings.AxisStyle }
          Line
              { X1 = settings.MaximumX
                X2 = settings.MaximumX
                Y1 = settings.MinimumY
                Y2 = settings.MaximumY
                Style = settings.AxisStyle }
          Line
              { X1 = settings.XAxisStartOverride |> Option.defaultValue settings.MinimumX
                X2 = settings.XAxisEndOverride |> Option.defaultValue settings.MaximumX
                Y1 = settings.MaximumY
                Y2 = settings.MaximumY
                Style = settings.AxisStyle }

          yield! createLabels settings minValue maxValue

          yield! createCurrentLine settings minValue maxValue data.Entries
          yield!
              createMovingAverageLines
                  settings
                  minValue
                  maxValue
                  data.ItemCount
                  shortMoveAverageItems
                  longMoveAverageItems
          yield! createCandleSticks settings data.ItemCount minValue maxValue data.Entries ]

    let createFromStockData (parameters: StockDataParameters) =
        let baseData = parameters.Data.BaseInstrumentEntries()

        let auxData = parameters.Data.AuxInstrumentEntries()

        let instrument =
            ({ Name = None
               Symbol = ""
               Type = InstrumentType.Stock
               Entries = baseData }
            : Instrument)

        let maItems =
            parameters.Data.All()
            |> List.map (fun d ->
                { Symbol = ""
                  Date = d.EntryDate
                  Price = d.CloseValue }
                : BasicInputItem)

        // TODO make window size configurable
        let ma50 =
            SimpleMovingAverage.generate { WindowSize = 50 } maItems
            |> List.take parameters.Data.BaseData.Length
            |> List.rev


        // TODO make window size configurable
        let ma200 =
            SimpleMovingAverage.generate { WindowSize = 200 } maItems
            |> List.take parameters.Data.BaseData.Length
            |> List.rev

        create parameters.ChartSettings instrument ma50 ma200
