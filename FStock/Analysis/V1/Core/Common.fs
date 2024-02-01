namespace FStock.Analysis.V1.Core

open Microsoft.FSharp.Core

[<AutoOpen>]
module Common =

    open System

    type BasicInputItem = { Symbol: string; Date: DateTime; Price: decimal }

    let prepend<'T> (item: 'T) (values: 'T list) = item :: values
