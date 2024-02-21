namespace FStock.Analysis.V1.Visualizations.Charts

open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module VolumeChart =

    open FSVG

    type ChartItem =
        {
            VolumeValue: decimal
        }
    
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

    let createBars (parameters: Parameters) =
        let data = parameters.Data.BaseData |> List.map (fun d -> d.VolumeValue)

        let maxValue = data |> List.max
        let minValue = 0m

        let sectionPadding = 0.5

        let sectionWidth =
            (parameters.MaximumX - parameters.MinimumX)
            / float parameters.Data.BaseData.Length

        let barWidth = sectionWidth - (sectionPadding * 2.)

        let barStyle =
            { Style.Default() with
                Opacity = Some 1.
                Fill = Some "blue" }

        data
        |> List.mapi (fun i d ->
            createBar
                d
                minValue
                maxValue
                parameters.MinimumY
                parameters.MaximumY
                (parameters.MinimumX + (sectionWidth * float i) + sectionPadding)
                barWidth
                true
                barStyle)


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

          yield! createBars parameters ]
