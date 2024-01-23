namespace FStock.Analysis.V1.TechnicalIndicators

[<RequireQualifiedAccess>]
module RelativeStrengthIndex =

    type Parameters =
        { Periods: int }

        static member Default() = { Periods = 14 }

    
    type Step1Item =
        {
            PeriodNumber: int
            Price: decimal
            Difference: decimal
            Gain: decimal
            Loss: decimal
        }
    
    let step1 (values: decimal list) =
        values
        |> List.fold (fun (prevValue, r) v ->
            match prevValue with
            | None -> 0m
            | Some -> 
            
             ) (None, [])
    
    let calculate _ =

        let averageGain = 0m
        let averageLoss = 0m




        ()



    ()
