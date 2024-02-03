namespace FStock.Data


module Domain =
    
    
    /// <summary>
    /// Represents a want to define open, high, low, close and adjusted close values
    /// </summary>
    type [<RequireQualifiedAccess>] OHLCValue =
        | Open
        | High
        | Low
        | Close
        | AdjustedClose
        
        
    [<AutoOpen>]    
    module Extensions =
        
        
        type FStock.Data.Persistence.Records.Stock with
        
            member s.GetOHLCValue(ohlc: OHLCValue) =
                match ohlc

