namespace FStock.Visualizations

open FSVG
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
          Volume: decimal
          MovingAverage200: decimal
          MovingAverage50: decimal }

        member dp.IsUp() = dp.Close > dp.Open

    type ScaledPoint =
        { Point: DataPoint
          Open: decimal
          Close: decimal
          High: decimal
          Low: decimal
          AdjClose: decimal
          MovingAverage200: decimal
          MovingAverage50: decimal }

    type CandleStickChartSettings = { ValuePaddingPercent: decimal }

    let scale
        (size: decimal)
        (topMargin: decimal)
        (diff: decimal)
        (max: decimal)
        (min: decimal)
        (sizeDiff: Decimal)
        (value: decimal)
        =
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

        let adjVale =
            (value + (sizeDiff * 2m)) - diff

        let flippedValue = diff - adjVale - sizeDiff

        //(flippedValue / diff) * size + topMargin
        (((((value - min) / diff) - 1m) * -1m) * size)
        + topMargin
        |> fun v ->
            //printfn $"*** value: {value} - max {max} size diff {sizeDiff} adj {adjVale} flipped {flippedValue} - {v}"
            v

    let scaleVolume (maxVolume: decimal) (size: decimal) (top: decimal) (value: decimal) = (value / maxVolume) * size

    let scalePoint
        (size: decimal)
        (topMargin: Decimal)
        (diff: decimal)
        (max: decimal)
        (min: decimal)
        (sizeDiff: decimal)
        (point: DataPoint)
        =
        ({ Point = point
           Open = scale size topMargin diff max min sizeDiff point.Open
           Close = scale size topMargin diff max min sizeDiff point.Close
           High = scale size topMargin diff max min sizeDiff point.High
           Low = scale size topMargin diff max min sizeDiff point.Low
           AdjClose = scale size topMargin diff max min sizeDiff point.AdjClose
           MovingAverage200 = scale size topMargin diff max min sizeDiff point.MovingAverage200
           MovingAverage50 = scale size topMargin diff max min sizeDiff point.MovingAverage50 }: ScaledPoint)

    let toSvgPoint (x: decimal) (scaledPoint: ScaledPoint) =
        SvgPoint.Create(float x, float scaledPoint.Close)

    let generateCandleStickChart (settings: CandleStickChartSettings) (points: DataPoint list) =

        // TODO make configurable.
        let height = 90m - 10m
        let width = 90m - 10m

        let topMargin = 10m

        let maxPointValue =
            points
            |> List.maxBy (fun p ->
                [ p.High
                  p.MovingAverage200
                  p.MovingAverage50 ]
                |> List.max)
            |> fun dp ->
                [ dp.High
                  dp.MovingAverage200
                  dp.MovingAverage50 ]
                |> List.max

        let minPointValue =
            points
            |> List.minBy (fun p ->
                [ p.Low
                  p.MovingAverage200
                  p.MovingAverage50 ]
                |> List.min)
            |> fun dp ->
                [ dp.Low
                  dp.MovingAverage200
                  dp.MovingAverage50 ]
                |> List.min

        // c is the average of max and min values.
        let c = (maxPointValue - minPointValue) / 2m

        // Size difference is calculated from c so it is even for max and min.
        let sizeDiff =
            ((c / 100m) * settings.ValuePaddingPercent)

        let maxVolume =
            points
            |> List.maxBy (fun p -> p.Volume)
            |> fun r -> r.Volume

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

        //printfn $"*** Diff: {diff} Max: {max} Min: {min}"

        let maxCount = points.Length

        let widthSize = width / (decimal maxCount)

        let scaledPoints =
            points
            |> List.map (scalePoint height topMargin diff max min sizeDiff)

        let svgPoints =
            scaledPoints
            |> List.mapi (fun i sp ->
                //let adjOpen =

                sp
                |> toSvgPoint ((decimal i * widthSize) + (widthSize / 2m) + 10m))
            |> SvgPoints.Create

        let avg200Points =
            scaledPoints
            |> List.mapi (fun i sp ->
                SvgPoint.Create(float ((decimal i * widthSize) + (widthSize / 2m) + 10m), float sp.MovingAverage200))
            |> SvgPoints.Create

        let avg50Points =
            scaledPoints
            |> List.mapi (fun i sp ->
                SvgPoint.Create(float ((decimal i * widthSize) + (widthSize / 2m) + 10m), float sp.MovingAverage50))
            |> SvgPoints.Create

        let upStyle =
            { Style.Default() with
                StrokeWidth = Some 0.1
                Stroke = Some "black"
                Fill = Some "green" }

        let downStyle =
            { Style.Default() with
                StrokeWidth = Some 0.1
                Stroke = Some "black"
                Fill = Some "red" }

        let candleSticks =
            scaledPoints
            |> List.mapi (fun i sp ->
                let xCenter =
                    (decimal i * widthSize) + (widthSize / 2m)

                let x =
                    (float (decimal i * widthSize)) + 10.

                let y = float sp.High

                let style =
                    match sp.Point.IsUp() with
                    | true -> upStyle
                    | false -> downStyle

                rect style 0. 0. (float (sp.Low - sp.High)) (float widthSize) x y)

        let volumes =
            points
            |> List.mapi (fun i p ->
                let h =
                    scaleVolume maxVolume (110m - 95m) 95m p.Volume
                    |> float

                let w = widthSize |> float
                let x = 10. + (float i * w)
                let y = (95. - h) + 15.

                rect
                    { Style.Default() with
                        Fill = Some "blue"
                        StrokeWidth = Some 0.1
                        Stroke = Some "black" }
                    0.
                    0.
                    h
                    w
                    x
                    y)

        let yPointSplit = diff / 10m

        let markerStyle =
            { Style.Default() with
                Stroke = Some "gray"
                StrokeWidth = Some 0.1 }

        let yMarkers =
            [ 0..10 ]
            |> List.collect (fun i ->
                let y = ((80. / 10.) * float i) + 10.

                let t =
                    (max - yPointSplit * decimal i).ToString("0.00")

                [ text
                      ({ Style.Default() with
                          Fill = Some "black"
                          GenericValues =
                              [ "font-size", "2px"
                                "font-family", "monospace"
                                "alignment-baseline", "middle" ]
                              |> Map.ofList })
                      2.
                      y
                      [ TextType.Literal t ]
                  line markerStyle 9. y 90. y ])

        let xMarkers =
            points
            |> List.mapi (fun i p ->
                let x = (float widthSize * float i) + 10.

                text
                    ({ Style.Default() with
                        Fill = Some "black"
                        GenericValues =
                            [ "font-size", "2px"
                              "font-family", "monospace"
                              "alignment-baseline", "middle"
                              "transform", "rotate(30deg) translateX(56px) translateY(-20px)" ]
                            |> Map.ofList })
                    x
                    112.
                    [ TextType.Literal <| p.Date.ToString("dd-MM-yy") ])

        let axisStyle =
            style Map.empty None (Some "black") (Some 0.1) (Some 1.)

        let line1 =
            style Map.empty None (Some "black") (Some 0.1) (Some 1.)

        let avg200Line =
            style Map.empty None (Some "blue") (Some 0.1) (Some 1.)

        let avg50Line =
            style Map.empty None (Some "orange") (Some 0.1) (Some 1.)

        svg [ line axisStyle 10. 10. 10. 90.
              line axisStyle 10. 90. 90. 90.

              line axisStyle 10. 95. 10. 110.
              line axisStyle 10. 110. 90. 110.

              yield! yMarkers
              yield! xMarkers
              path line1 <| createBezierCommands svgPoints
              yield! candleSticks
              path avg50Line <| createBezierCommands avg50Points
              path avg200Line
              <| createBezierCommands avg200Points
              yield! volumes ]

    
    module DistributionChart =
            
        type Bucket =
            { Min: decimal
              Max: decimal
              Count: int }

        let group (bucketCount: int) (points: decimal list) =
            let max = (points |> List.max) + 0.1m
            let min = points |> List.min

            let diff = max - min

            let size = diff / (decimal bucketCount)

            //let buckets =
            List.init bucketCount (fun i -> ((decimal i * size) + min, (decimal (i + 1) * size) + min))
            |> List.map (fun (mi, ma) ->
                { Min = mi
                  Max = ma
                  Count =
                    points
                    |> List.filter (fun v -> v >= mi && v < ma)
                    |> List.length })

        let scaleCount (maxCount: float) (size: float) (top: float) (value: float) = (value / maxCount) * size
            
        let generate (points: decimal list) =
            
            let buckets = group 10 points
        
            let max = buckets |> List.maxBy (fun b -> b.Count) |> fun b -> b.Count
    
            let count = buckets |> List.sumBy (fun b -> b.Count)
            
            let widthSize = (80. / float 10.)
                
            let bars =
                buckets
                |> List.mapi (fun i b ->
                    let h =
                        scaleCount (float max) (80.) 10. (float b.Count) 
                        |> float

                    let w = widthSize |> float
                    let x = 10. + (float i * w)
                    let y = (90. - h) // + 15.

                    rect
                        { Style.Default() with
                            Fill = Some "blue"
                            StrokeWidth = Some 0.1
                            Stroke = Some "black" }
                        0.
                        0.
                        h
                        w
                        x
                        y)
                
        
            let axisStyle =
                style Map.empty None (Some "black") (Some 0.1) (Some 1.)
            
            svg [ line axisStyle 10. 10. 10. 90.
                  line axisStyle 10. 90. 90. 90.
                  yield! bars ]
            

        let distributionChart (points: decimal list) =

            // TODO make configurable.
            let height = 90m - 10m
            let width = 90m - 10m

            let topMargin = 10m
                    
            let max = points |> List.max
            
            let min = points |> List.min
            
            // c is the average of max and min values.
            let c = (max - min) / 2m

            // Size difference is calculated from c so it is even for max and min.
            let sizeDiff =
                ((c / 100m) * 20m)

            //let 
            
            let scaledPoints = points |> List.map scale 

                    
            
            ()
            
            