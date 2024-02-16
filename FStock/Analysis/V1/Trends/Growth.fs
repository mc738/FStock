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
    
    type TrendItem =
        {
            From: decimal
            To: decimal
            GrowthTrend: GrowthTrend
            Velocity: Velocity
        }
        
    let createTrendItems (values: decimal list) =
        values
        |> List.pairwise
        |> List.map (fun (f, t) ->
            let gt =
                

            
            
            
            
            
            ())
        
    
    

