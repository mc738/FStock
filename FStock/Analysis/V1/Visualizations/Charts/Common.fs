﻿namespace FStock.Analysis.V1.Visualizations.Charts

[<AutoOpen>]
module Common =

    open FSVG
    open FSVG.Charts

    type StockData =
        { BaseData: FStock.Data.Persistence.Records.Stock list
          AuxData: FStock.Data.Persistence.Records.Stock list }
        
        member sd.All() = (sd.BaseData @ sd.AuxData) |> List.sortBy (fun d -> d.EntryDate) 
    
    let normalizedValueToPosition (minPos: float) (maxPos: float) (normalizedValue: float) =
        minPos + ((normalizedValue / 100.) * (maxPos - minPos))

    let normalizeXValue (value: decimal) (minValue: decimal) (maxValue: decimal) (minPos: float) (maxPos: float) =
        ({ MaxValue = maxValue
           MinValue = minValue
           Value = value }
        : NormalizerParameters<decimal>)
        |> rangeNormalizer<decimal> float
        |> normalizedValueToPosition minPos maxPos

    let normalizeYValue
        (value: decimal)
        (minValue: decimal)
        (maxValue: decimal)
        (minPos: float)
        (maxPos: float)
        (flipValue: bool)
        =
        ({ MaxValue = maxValue
           MinValue = minValue
           Value = value }
        : NormalizerParameters<decimal>)
        |> rangeNormalizer<decimal> float
        |> fun nv ->
            match flipValue with
            | true -> 100. - nv
            | false -> nv
        |> normalizedValueToPosition minPos maxPos

    let createBar
        (value: decimal)
        (minValue: decimal)
        (maxValue: decimal)
        (minYPos: float)
        (maxYPos: float)
        (xPos: float)
        (width: float)
        (flipValue: bool)
        (style: Style)
        =
        // First normalize the value.
        let nv =
            ({ MaxValue = maxValue
               MinValue = minValue
               Value = value }
            : NormalizerParameters<decimal>)
            |> rangeNormalizer<decimal> float

        let actualHeight = maxYPos - minYPos

        let (height, y) =
            let h = (nv / 100.) * actualHeight

            match flipValue with
            | true -> h, minYPos + actualHeight - h
            | false -> h, minYPos


        Rect
            { Height = height
              Width = width
              X = xPos
              Y = y
              RX = 0
              RY = 0
              Style = style }

    let defaultAxisStyle =
        { Fill = None
          Stroke = Some "black"
          StrokeWidth = Some 0.1
          StrokeLineCap = None
          StrokeDashArray = None
          Opacity = Some 1.
          GenericValues = Map.empty }
    