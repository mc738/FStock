namespace FStock.Modeling.V1

[<RequireQualifiedAccess>]
module Data =
    
    open System
    open Freql.Sqlite
    open FStock.Data
    open FStock.Data.Store
    open FStock.Data.Domain
    
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
               AdjustedClose = s.AdjustedCloseValue
               Volume = s.VolumeValue }
            : CurrentPosition))

    let getHistoricPositions (store: FStockStore) (filter: HistoricPositionFilter) : HistoricPosition list =
        // TODO add cache.
        
        []