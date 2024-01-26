namespace FStock.Analysis.V1.Visualizations.Charts

[<RequireQualifiedAccess>]
module VolumeChart =

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
                Y1 = normalizeYValue 25m 0m 100m parameters.MinimumY parameters.MaximumY true
                Y2 = normalizeYValue 75m 0m 100m parameters.MinimumY parameters.MaximumY true
                Style = parameters.AxisStyle } ]
