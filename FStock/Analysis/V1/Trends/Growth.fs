namespace FStock.Analysis.V1.Trends

[<RequireQualifiedAccess>]
module Growth =
    
    type [<RequireQualifiedAccess>] GrowthTrend =
        | Increasing
        | Decreasing
        | Stalled
        
    type [<RequireQualifiedAccess>] Velocity =
        | Accelerating
        | Decelerating
        | Stable
    
    ()

