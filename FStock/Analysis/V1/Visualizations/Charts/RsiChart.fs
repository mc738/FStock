namespace FStock.Analysis.V1.Visualizations.Charts

[<RequireQualifiedAccess>]
module RsiChart =
    
    
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
          AxisStyle: Style }

    let create (parameters: Parameters) =
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

          // Test
          Line
              { X1 = parameters.MinimumX
                X2 = parameters.MaximumX
                Y1 = normalizeYValue 30m 0m 100m parameters.MinimumY parameters.MaximumY true
                Y2 = normalizeYValue 30m 0m 100m parameters.MinimumY parameters.MaximumY true
                Style =
                  { parameters.AxisStyle with
                      Stroke = Some "blue"
                      StrokeWidth = Some 0.5
                      StrokeLineCap = Some StrokeLineCap.Butt
                      StrokeDashArray = Some [ 2; 2 ]
                      GenericValues = [ "stroke-dashoffset", "1" ] |> Map.ofList } }

          Line
              { X1 = parameters.MinimumX
                X2 = parameters.MaximumX
                Y1 = normalizeYValue 70m 0m 100m parameters.MinimumY parameters.MaximumY true
                Y2 = normalizeYValue 70m 0m 100m parameters.MinimumY parameters.MaximumY true
                Style =
                  { parameters.AxisStyle with
                      Stroke = Some "blue"
                      StrokeWidth = Some 0.5
                      StrokeLineCap = Some StrokeLineCap.Butt
                      StrokeDashArray = Some [ 2; 2 ]
                      GenericValues = [ "stroke-dashoffset", "1" ] |> Map.ofList } }

          Rect
              { Height = normalizeYValue 40m 0m 100m 0 (parameters.MaximumY - parameters.MinimumY) false
                Width = parameters.MaximumX - parameters.MinimumX
                X = parameters.MinimumX
                Y = normalizeYValue 30m 0m 100m parameters.MinimumY parameters.MaximumY false
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
    

