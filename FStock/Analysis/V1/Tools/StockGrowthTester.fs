namespace FStock.Analysis.V1.Tools

open System
open FStock.Data.Domain

[<RequireQualifiedAccess>]
module StockGrowthTester =
    
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
        
        $"{tableNamePrefix}_{date:ddMMyy}_"
        
    
    
    let createParameters =
        
        ()
    
    let run () =
        
        
        GrowthTester.run
        
        ()
    
    
    ()

