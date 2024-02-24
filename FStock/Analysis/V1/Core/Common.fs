namespace FStock.Analysis.V1.Core

open FStock.Data.Domain
open Microsoft.FSharp.Core

[<AutoOpen>]
module Common =

    open System

    type BasicInputItem =
        { Symbol: string
          Date: DateTime
          Price: decimal }

    type Instrument =
        { Name: string option
          Symbol: string
          Type: InstrumentType
          Entries: InstrumentPositionEntry list }

    and InstrumentPositionEntry =
        { Date: DateTime
          Open: decimal
          High: decimal
          Low: decimal
          Close: decimal
          AdjustedClose: decimal
          Volume: decimal }
        
        member ipe.GetOHLCValue(ohlc: OHLCValue) =
            match ohlc with
            | OHLCValue.Open -> ipe.Open
            | OHLCValue.High -> ipe.High
            | OHLCValue.Low -> ipe.Low
            | OHLCValue.Close -> ipe.Close
            | OHLCValue.AdjustedClose -> ipe.AdjustedClose
            | OHLCValue.Volume -> ipe.Volume

    and InstrumentType = | Stock

    let prepend<'T> (item: 'T) (values: 'T list) = item :: values
