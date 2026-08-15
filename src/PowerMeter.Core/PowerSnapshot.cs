namespace PowerMeter.Core
{
    /// <summary>
    /// あるスコープについての集計結果。エネルギー量はすべて W に換算済み。
    /// </summary>
    public readonly struct PowerSnapshot
    {
        public PowerSnapshot(
            bool isValid,
            int networkCount,
            double capacityWatt,
            double generationWatt,
            double consumptionWatt,
            double servedWatt,
            double satisfactionRatio,
            double accumulatedJoule)
        {
            IsValid = isValid;
            NetworkCount = networkCount;
            CapacityWatt = capacityWatt;
            GenerationWatt = generationWatt;
            ConsumptionWatt = consumptionWatt;
            ServedWatt = servedWatt;
            SatisfactionRatio = satisfactionRatio;
            AccumulatedJoule = accumulatedJoule;
        }

        /// <summary>集計対象が存在したか。false の場合、他の値はすべて 0。</summary>
        public bool IsValid { get; }

        /// <summary>集計対象になった電力網の数。</summary>
        public int NetworkCount { get; }

        /// <summary>最大発電能力 [W]。</summary>
        public double CapacityWatt { get; }

        /// <summary>実発電量 [W]。供給 + 充電 - 放電。</summary>
        public double GenerationWatt { get; }

        /// <summary>消費側の需要 [W]。</summary>
        public double ConsumptionWatt { get; }

        /// <summary>実際に供給された量 [W]。</summary>
        public double ServedWatt { get; }

        /// <summary>充足率（0.0〜1.0）。需要が 0 のときは 1.0。</summary>
        public double SatisfactionRatio { get; }

        /// <summary>蓄電池の蓄電量 [J]。tick 換算の対象外。</summary>
        public double AccumulatedJoule { get; }

        /// <summary>集計対象が存在しないことを表すスナップショット。</summary>
        public static PowerSnapshot Invalid => default;
    }
}
