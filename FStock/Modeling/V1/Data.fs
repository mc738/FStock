namespace FStock.Modeling.V1

open System
open FStock.Data
open FStock.Data.Store
open Freql.Sqlite

[<RequireQualifiedAccess>]
module Data =
    
    let getCurrentPosition (store: FStockStore) (openPosition: OpenPosition) (date: DateTime) =
        // TODO add cache?
        store.GetStockForDate(openPosition.Symbol, date)
        |> Option.map (fun s ->
            ({ Symbol = s.Symbol
               Date = s.EntryDate
               Open = s.OpenValue
               High = s.HighValue
               Low = s.LowValue
               Close = s.CloseValue
               AdjustedClose = s.AdjustedCloseValue }
            : CurrentPosition))

    let getHistoricPositions (store: FStockStore) (filter: HistoricPositionFilter) : HistoricPosition list =
        // TODO add cache.
        
        []