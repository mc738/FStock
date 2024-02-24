namespace FStock.Analysis.V1.Core

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

    and InstrumentType = | Stock

    let prepend<'T> (item: 'T) (values: 'T list) = item :: values
