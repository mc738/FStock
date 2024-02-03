namespace FStock.Modeling.V1

open FStock.Data.Domain

[<RequireQualifiedAccess>]
module Conditions =
        
        open FStock.Data.Domain
        
        /// <summary>
        /// A behaviour to check the current position's low value vs a fixed lose.
        /// This is to simulate if a stop-loss has been hit.
        /// </summary>
        /// <param name="stopLoss"></param>
        let ``fixed stop-loss based on low value`` stopLoss =
            PositionCondition.FixedLoss(stopLoss, ConditionValueMapper.Value OHLCValue.Low)

        /// <summary>
        /// A behaviour to check the current position's low value vs a percentage lose.
        /// This is to simulate if a stop-loss has been hit.
        /// </summary>
        /// <param name="stopLossPercent"></param>
        let ``percentage stop-loss based on low value`` stopLossPercent =
            PositionCondition.PercentageLoss(stopLossPercent, ConditionValueMapper.Value OHLCValue.Low)

        let ``fixed take profit based on high value`` takeProfit =
            PositionCondition.FixedValue(takeProfit, ConditionValueMapper.Value OHLCValue.High)

        let ``percentage take profit based on high value`` takeProfitPercent =
            PositionCondition.PercentageGrowth(takeProfitPercent, ConditionValueMapper.Value OHLCValue.High)
