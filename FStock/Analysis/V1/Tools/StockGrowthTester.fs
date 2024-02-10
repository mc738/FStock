namespace FStock.Analysis.V1.Tools

[<RequireQualifiedAccess>]
module StockGrowthTester =

    open System
    open FStock.Data
    open FStock.Data.Domain
        
    type Parameters =
        {
            BuyOHLCValue: OHLCValue
            SellOHLCValue: OHLCValue
            TakeProfit: decimal option
            StopLoss: decimal option
            MaxPeriods: int
        }
        
        
    //let getOHLC
    
    let tableNamePrefix = "stock_result"
    
    let createTableName (parameters: Parameters) (date:DateTime) =
        let ohlcPart (ohlcValue: OHLCValue) =
            match ohlcValue with
            | OHLCValue.Open -> "o"
            | OHLCValue.High -> "h"
            | OHLCValue.Low -> "l"
            | OHLCValue.Close -> "c"
            | OHLCValue.AdjustedClose -> "ac"
            | OHLCValue.Volume -> "v"
        
        let takeProfitPart =
            match parameters.TakeProfit with
            | Some tp -> $"tp_{tp}"
            | None -> "tp_no"
            
        let stopLossPart =
            match parameters.StopLoss with
            | Some sl -> $"sl_{sl}"
            | None -> "sl_no"
        
        $"{tableNamePrefix}_{date:ddMMyy}_{takeProfitPart}_{stopLossPart}_mp_{}_b_{ohlcPart parameters.BuyOHLCValue}_s_{ohlcPart parameters.SellOHLCValue}"
        
    
    
    let createParameters =
        
        ()
    
    let run (store: Store.FStockStore) =
        
        
        GrowthTester.run
        
        ()
    
    
    ()

