namespace FStock.Analysis.V1.Visualizations.Charts

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
          AxisStyle: Style }

    let create (parameters: Parameters) =

        [ // First create the axis
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

          createBar 25m 0m 100m parameters.MinimumY parameters.MaximumY (parameters.MinimumX) 10. true testStyle

          createBar 50m 0m 100m parameters.MinimumY parameters.MaximumY (parameters.MinimumX + 10.) 10. true testStyle
          createBar 75m 0m 100m parameters.MinimumY parameters.MaximumY (parameters.MinimumX + 20.) 10. true testStyle

          createBar 25m 0m 100m parameters.MinimumY parameters.MaximumY (parameters.MinimumX + 50.) 10. false testStyle

          createBar 50m 0m 100m parameters.MinimumY parameters.MaximumY (parameters.MinimumX + 60.) 10. false testStyle
          createBar 75m 0m 100m parameters.MinimumY parameters.MaximumY (parameters.MinimumX + 70.) 10. false testStyle ]
