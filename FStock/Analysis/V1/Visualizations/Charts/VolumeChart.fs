namespace FStock.Analysis.V1.Visualizations.Charts

open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module VolumeChart =

    open FSVG

    type ChartItem = { VolumeValue: decimal }

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

    let createBars (settings: ChartSettings) (data: decimal list) =
        //let data = parameters.Data.BaseData |> List.map (fun d -> d.VolumeValue)

        let maxValue = data |> List.max
        let minValue = 0m

        let sectionPadding = 0.5

        let sectionWidth =
            (settings.MaximumX - settings.MinimumX)
            / float data.Length

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
                settings.MinimumY
                settings.MaximumY
                (settings.MinimumX + (sectionWidth * float i) + sectionPadding)
                barWidth
                true
                barStyle)

    let create (settings: ChartSettings) (data: decimal list) =
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

          yield! createBars settings data ]
        
    let createFromStockData (parameters: StockDataParameters) =
        let data = parameters.Data.BaseData |> List.map (fun d -> d.VolumeValue)
        
        create parameters.ChartSettings data