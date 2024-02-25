namespace FStock.Analysis.V1.Tools

module ChartGenerator =
    
    [<RequireQualifiedAccess>]
    type ChartType =
        | Price
        | Volume
        | Macd
        | Rsi
    
    and PriceChartSettings =
        {
            Height: float
            
        }
     
    
    and GeneralSettings =
        {
            Width: float
            
        }
    
    
    ()

