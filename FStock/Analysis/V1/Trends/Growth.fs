namespace FStock.Analysis.V1.Trends

[<RequireQualifiedAccess>]
module Growth =

    [<RequireQualifiedAccess>]
    type GrowthTrend =
        | Increasing
        | Decreasing
        | Stalled

    [<RequireQualifiedAccess>]
    type Velocity =
        | Accelerating
        | Decelerating
        | Stable

    type TrendItem =
        { From: decimal
          To: decimal
          GrowthTrend: GrowthTrend
          Velocity: Velocity }

    let createTrendItems (values: decimal list) =
        values
        |> List.pairwise
        |> List.fold (fun (acc, (f, t)) ->
            let gt =
                if f > t then GrowthTrend.Decreasing
                else if f < t then GrowthTrend.Increasing
                else GrowthTrend.Stalled

            { From = f
              To = t
              GrowthTrend = gt
              Velocity = Velocity.Stable })
