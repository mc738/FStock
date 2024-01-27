namespace FStock.Analysis.V1.Visualizations.Charts

open FStock.Analysis.V1.Core
open FStock.Analysis.V1.TechnicalIndicators

[<RequireQualifiedAccess>]
module MacdChart =

    open FSVG

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

    let createMcadLine (parameters: Parameters) (data: MovingAverageConvergenceDivergence.MacdItem list) =
        let sectionWidth =
            (parameters.MaximumX - parameters.MinimumX) / float parameters.Data.BaseData.Length
        
        let maxItem =
            data
            |> List.maxBy (fun d ->
                if d.MacdValue > d.SignalEma then
                    d.MacdValue
                else
                    d.SignalEma)

        let minItem =
            data
            |> List.minBy (fun d ->
                if d.MacdValue < d.SignalEma then
                    d.MacdValue
                else
                    d.SignalEma)

        let (maxValue, minValue) =
            if maxItem.MacdValue > (-1m * minItem.MacdValue) then
                let v = maxItem.MacdValue + ((maxItem.MacdValue / 100m) * 10m)
                
                v, v * -1m
            else
                let v = (minItem.MacdValue * -1m) + (((minItem.MacdValue * -1m) / 100m) * 10m)
                
                v, v * -1m

        let (macdPoints, signalPoints) =
            data
            |> List.mapi (fun i item ->

                let x =
                    parameters.MinimumX
                      + (float i * sectionWidth)
                      + (sectionWidth / 2.)
                
                ({ X = x
                   Y = normalizeYValue item.MacdValue minValue maxValue parameters.MinimumY parameters.MaximumY true }
                : SvgPoint),
                ({ X = x
                   Y = normalizeYValue item.SignalEma minValue maxValue parameters.MinimumY parameters.MaximumY true }
                : SvgPoint))
            |> List.unzip

        let basicStyle =
            { Style.Default() with
                Opacity = Some 1.
                StrokeWidth = Some 0.1 }

        [ Path
              { Commands = macdPoints |> SvgPoints.Create |> Helpers.createBezierCommands
                Style = { basicStyle with Stroke = Some "blue" } }
          Path
              { Commands = signalPoints |> SvgPoints.Create |> Helpers.createBezierCommands
                Style =
                  { basicStyle with
                      Stroke = Some "orange" } } ]

    let create (parameters: Parameters) =
        let data =
            parameters.Data.All()
            |> List.map (fun d ->
                ({ Date = d.EntryDate
                   Price = d.CloseValue }
                : BasicInputItem))
            |> MovingAverageConvergenceDivergence.generate (MovingAverageConvergenceDivergence.Parameters.Default())
            |> List.take parameters.Data.BaseData.Length
            |> List.rev


        [ Line
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

          Line
              { X1 = parameters.MinimumX
                X2 = parameters.MaximumX
                Y1 = normalizeYValue 50m 0m 100m parameters.MinimumY parameters.MaximumY true
                Y2 = normalizeYValue 50m 0m 100m parameters.MinimumY parameters.MaximumY true
                Style =
                  { parameters.AxisStyle with
                      StrokeDashArray = Some [ 2; 2 ]
                      StrokeWidth = Some 0.5
                      GenericValues = [ "stroke-dashoffset", "1" ] |> Map.ofList } }
          yield! createMcadLine parameters data ]
