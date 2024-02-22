namespace FStock.Analysis.V1.Visualizations.Charts

open FStock.Analysis.V1.Core
open FStock.Analysis.V1.TechnicalIndicators
open FStock.Data.Persistence

[<RequireQualifiedAccess>]
module RsiChart =

    open FSVG

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

    let createRsiLine (settings: ChartSettings) (itemCount: int) (data: RelativeStrengthIndex.RsiItem list) =

        let sectionWidth = (settings.MaximumX - settings.MinimumX) / float itemCount

        Path
            { Commands =
                data
                |> List.mapi (fun i d ->
                    ({ X = settings.MinimumX + (float i * sectionWidth) + (sectionWidth / 2.)
                       Y = normalizeYValue d.Rsi 0m 100m settings.MinimumY settings.MaximumY true }
                    : SvgPoint))
                |> SvgPoints.Create
                |> Helpers.createStraightCommands
              Style =
                { Style.Default() with
                    Opacity = Some 1
                    StrokeWidth = Some 0.1
                    Stroke = Some "black" } }


    let create (settings: ChartSettings) (data: RelativeStrengthIndex.RsiItem list) =
        let itemCount = data.Length

        [ Line
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

          Line
              { X1 = settings.MinimumX
                X2 = settings.MaximumX
                Y1 = normalizeYValue 30m 0m 100m settings.MinimumY settings.MaximumY true
                Y2 = normalizeYValue 30m 0m 100m settings.MinimumY settings.MaximumY true
                Style =
                  { settings.AxisStyle with
                      Stroke = Some "blue"
                      StrokeWidth = Some 0.5
                      StrokeLineCap = Some StrokeLineCap.Butt
                      StrokeDashArray = Some [ 2; 2 ]
                      GenericValues = [ "stroke-dashoffset", "1" ] |> Map.ofList } }

          Line
              { X1 = settings.MinimumX
                X2 = settings.MaximumX
                Y1 = normalizeYValue 70m 0m 100m settings.MinimumY settings.MaximumY true
                Y2 = normalizeYValue 70m 0m 100m settings.MinimumY settings.MaximumY true
                Style =
                  { settings.AxisStyle with
                      Stroke = Some "blue"
                      StrokeWidth = Some 0.5
                      StrokeLineCap = Some StrokeLineCap.Butt
                      StrokeDashArray = Some [ 2; 2 ]
                      GenericValues = [ "stroke-dashoffset", "1" ] |> Map.ofList } }

          Rect
              { Height = normalizeYValue 40m 0m 100m 0 (settings.MaximumY - settings.MinimumY) false
                Width = settings.MaximumX - settings.MinimumX
                X = settings.MinimumX
                Y = normalizeYValue 30m 0m 100m settings.MinimumY settings.MaximumY false
                RX = 0.
                RY = 0.
                Style =
                  { Fill = Some "blue"
                    Stroke = None
                    StrokeWidth = None
                    StrokeLineCap = None
                    StrokeDashArray = None
                    Opacity = Some 0.2
                    GenericValues = Map.empty } }

          createRsiLine settings itemCount data ]

    let createFromStockData (parameters: StockDataParameters) =
        let settings = parameters.ChartSettings

        let rsiData =
            parameters.Data.All()
            |> List.map (fun d ->
                ({ Symbol = ""
                   Date = d.EntryDate
                   Price = d.CloseValue }
                : BasicInputItem))
            |> RelativeStrengthIndex.generate (RelativeStrengthIndex.Parameters.Default())
            |> List.take parameters.Data.BaseData.Length
            |> List.rev

        create settings rsiData
