namespace FStock.Visualizations

open Microsoft.FSharp.Core

module Charts =

    open System
    open FSVG
    open FSVG.Helpers
    open FSVG.Dsl

    type DataPoint =
        { Date: DateTime
          Open: decimal
          Close: decimal
          High: decimal
          Low: decimal
          AdjClose: decimal
          Volume: decimal }

        member dp.IsUp() = dp.Close > dp.Open

    type ScaledPoint =
        { Point: DataPoint
          Open: decimal
          Close: decimal
          High: decimal
          Low: decimal
          AdjClose: decimal }

    type CandleStickChartSettings = { ValuePaddingPercent: decimal }

    let scale (size: decimal) (topMargin: decimal) (diff: decimal) (max: decimal) (min: decimal) (sizeDiff: Decimal) (value: decimal) =
        // find position within range

        //
        
        // Attempt -
        // (value / max) (to get it's percent) (- diff to try and scale better)
        // - 1 * -1 (to flip it)
        // * size
        
        // Attempt 2 -
        // (value - min) / diff (or max - min) # (basically scaling to 0).
        // - 1 # flip the percent
        // * -1 # Make positive
        // * size # scale to actual size
        // + topMargin # position.
        
        let adjVale = (value + (sizeDiff * 2m)) - diff

        let flippedValue = diff - adjVale - sizeDiff

        //(flippedValue / diff) * size + topMargin
        (((((value - min) / diff) - 1m) * -1m) * size) + topMargin
        |> fun v ->
            printfn $"*** value: {value} - max {max} size diff {sizeDiff} adj {adjVale} flipped {flippedValue} - {v}"
            v

    let scalePoint (size: decimal) (topMargin: Decimal) (diff: decimal) (max: decimal) (min: decimal) (sizeDiff: decimal) (point: DataPoint) =
        ({ Point = point
           Open = scale size topMargin diff max min sizeDiff point.Open
           Close = scale size topMargin diff max min sizeDiff point.Close
           High = scale size topMargin diff max min sizeDiff point.High
           Low = scale size topMargin diff max min sizeDiff point.Low
           AdjClose = scale size topMargin diff max min sizeDiff point.AdjClose }: ScaledPoint)

    let toSvgPoint (x: decimal) (scaledPoint: ScaledPoint) =
        SvgPoint.Create(float x, float scaledPoint.Close)

    let generateCandleStickChart (settings: CandleStickChartSettings) (points: DataPoint list) =

        // TODO make configurable.
        let height = 90m - 10m
        let width = 90m - 10m

        let topMargin = 10m

        let maxPointValue =
            points
            |> List.maxBy (fun p -> p.High)
            |> fun dp -> dp.High
            
        let minPointValue =
            points
            |> List.minBy (fun p -> p.Low)
            |> fun dp -> dp.Low
        
        // c is the average of max and min values.
        let c = (maxPointValue - minPointValue) / 2m 
        
        // Size difference is calculated from c so it is even for max and min.
        let sizeDiff = ((c / 100m) * settings.ValuePaddingPercent)
        
        let max = maxPointValue + sizeDiff
            //points
            //|> List.maxBy (fun p -> p.High)
            //|> fun dp ->
            //    dp.High + 2m
                //+ ((dp.High / 100m) * settings.ValuePaddingPercent)

        let min = minPointValue - sizeDiff
            //points
            //|> List.minBy (fun p -> p.Low)
            //|> fun dp ->
            //    dp.Low - 2m
                //- ((dp.Low / 100m) * settings.ValuePaddingPercent)

        let diff = max - min

        printfn $"*** Diff: {diff} Max: {max} Min: {min}"

        let maxCount = points.Length

        let widthSize = width / (decimal maxCount)

        let scaledPoints =
            points
            |> List.map (scalePoint height topMargin diff max min sizeDiff)

        let svgPoints =
            scaledPoints
            |> List.mapi (fun i sp ->
                //let adjOpen =

                sp |> toSvgPoint ((decimal i * widthSize) + (widthSize / 2m) + 10m))
            |> SvgPoints.Create

        let candleStickStyle1 =
            { Style.Default() with
                StrokeWidth = Some 0.1
                Stroke = Some "black" }

        let candleSticks =
            scaledPoints |> List.mapi (fun i sp ->
                let xCenter = (decimal i * widthSize) + (widthSize / 2m)
                
                let x = (float (decimal i * widthSize)) + 10.
                let y = float sp.High
                
                rect candleStickStyle1 0. 0. (float (sp.Low - sp.High)) (float widthSize) x y)

        let yPointSplit = diff / 10m
        
        let yMarkers =
            [0..10]
            |> List.map (fun i ->
                let y = ((80. / 10.) * float i) + 10.
                let t = (max - yPointSplit * decimal i).ToString("0.00")
                text ({ Style.Default() with Fill = Some "black"; GenericValues = [ "font-size", "1.5px" ] |> Map.ofList}) 5. y [ TextType.Literal t])
        
        let axisStyle =
            style Map.empty None (Some "black") (Some 0.1) (Some 1.)

        let line1 =
            style Map.empty None (Some "black") (Some 0.1) (Some 1.)

        svg [ line axisStyle 10. 10. 10. 90.
              line axisStyle 10. 90. 90. 90.
              path line1 <| createBezierCommands svgPoints
              yield! candleSticks
              yield! yMarkers ]
