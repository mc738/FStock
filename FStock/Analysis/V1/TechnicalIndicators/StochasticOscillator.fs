namespace FStock.Analysis.V1.TechnicalIndicators

module StochasticOscillator =

    open System    
    
    type Parameters =
        {
            WindowSize: int
        }
    
    type StochasticOscillatorItem =
        {
            Symbol: string
            EntryDate: DateTime
        }
   
    let calculateK (current: decimal) (previousHigh: decimal) (previousLow: decimal) =
        ((current - previousLow) / (previousHigh - previousLow)) * 100m
    
    
