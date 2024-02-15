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

    let createHistograms (parameters: Parameters) (minValue: decimal) (maxValue: decimal) (data: MovingAverageConvergenceDivergence.MacdItem list) =
        
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
            
            let prevHValue =
                match i > 0 with
                | true -> data.Item (i - 1) |> fun pv -> pv.MacdValue - pv.SignalEma
                | false -> 0m
                
            let hValue = d.MacdValue - d.SignalEma
            
            let opacity =
                match hValue > 0m, prevHValue > 0m, hValue >= prevHValue with
                | true, true, true -> 1.
                | true, true, false -> 0.5
                | false, false, false -> 1.
                | false, false, true -> 0.5
                | true, false, _ -> 1.
                | false, true, _ -> 1.
                
            match hValue > 0m with
            | true ->
                createBar
                    hValue
                    0m
                    maxValue
                    parameters.MinimumY
                    (parameters.MinimumY + ((parameters.MaximumY - parameters.MinimumY) / 2.))
                    (parameters.MinimumX + (sectionWidth * float i) + sectionPadding)
                    barWidth
                    true
                    { barStyle with Fill = Some "green"; Opacity = Some opacity }
            | false ->
                createBar
                    (hValue * -1m)
                    0m
                    maxValue
                    (parameters.MinimumY + ((parameters.MaximumY - parameters.MinimumY) / 2.))
                    parameters.MaximumY
                    (parameters.MinimumX + (sectionWidth * float i) + sectionPadding)
                    barWidth
                    false
                    { barStyle with Fill = Some "red"; Opacity = Some opacity })
        
    
    let createMcadLine (parameters: Parameters) (minValue: decimal) (maxValue: decimal) (data: MovingAverageConvergenceDivergence.MacdItem list) =
        let sectionWidth =
            (parameters.MaximumX - parameters.MinimumX) / float parameters.Data.BaseData.Length
        
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
                ({ Symbol = ""
                   Date = d.EntryDate
                   Price = d.CloseValue }
                : BasicInputItem))
            |> MovingAverageConvergenceDivergence.generate (MovingAverageConvergenceDivergence.Parameters.Default())
            |> List.take parameters.Data.BaseData.Length
            |> List.rev
            
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

          yield! createHistograms parameters minValue maxValue data
          
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
          yield! createMcadLine parameters minValue maxValue data ]
