namespace FStock.Data


module Domain =
    
    
    /// <summary>
    /// Represents a want to define open, high, low, close and adjusted close values.
    /// NOTE: this also includes adjusted and volume values for convince.
    /// </summary>
    type [<RequireQualifiedAccess>] OHLCValue =
        | Open
        | High
        | Low
        | Close
        | AdjustedClose
        | Volume
        
        member v.Serialize() =
            match v with
            | OHLCValue.Open -> "open"
            | OHLCValue.High -> "high"
            | OHLCValue.Low -> "low"
            | OHLCValue.Close -> "close"
            | OHLCValue.AdjustedClose -> "adj_close"
            | OHLCValue.Volume -> "volume"
        
        
        
    [<AutoOpen>]    
    module Extensions =
        
        
        type FStock.Data.Persistence.Records.Stock with
        
            member s.GetOHLCValue(ohlc: OHLCValue) =
                match ohlc with
                | OHLCValue.Open -> s.OpenValue
                | OHLCValue.High -> s.HighValue
                | OHLCValue.Low -> s.LowValue
                | OHLCValue.Close -> s.CloseValue
                | OHLCValue.AdjustedClose -> s.AdjustedCloseValue
                | OHLCValue.Volume -> s.VolumeValue

