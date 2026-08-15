namespace PowerMeter.Core
{
    /// <summary>
    /// あるスコープについての集計結果。電力はすべて W、エネルギーは J に換算済み。
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
            double chargeWatt,
            double dischargeWatt,
            double satisfactionRatio,
            double utilizationRatio,
            double storedJoule)
        {
            IsValid = isValid;
            NetworkCount = networkCount;
            CapacityWatt = capacityWatt;
            GenerationWatt = generationWatt;
            ConsumptionWatt = consumptionWatt;
            ServedWatt = servedWatt;
            ChargeWatt = chargeWatt;
            DischargeWatt = dischargeWatt;
            SatisfactionRatio = satisfactionRatio;
            UtilizationRatio = utilizationRatio;
            StoredJoule = storedJoule;
        }

        /// <summary>集計対象が存在したか。false の場合、他の値はすべて 0。</summary>
        public bool IsValid { get; }

        /// <summary>集計対象になった電力網の数。</summary>
        public int NetworkCount { get; }

        /// <summary>最大発電能力 [W]。ゲーム内表示の「発電性能」に対応する。</summary>
        public double CapacityWatt { get; }

        /// <summary>実発電量 [W]。供給 + 充電 - 放電。</summary>
        public double GenerationWatt { get; }

        /// <summary>消費側の需要 [W]。ゲーム内表示の「必要消費電力」に対応する。</summary>
        public double ConsumptionWatt { get; }

        /// <summary>実際に供給された量 [W]。</summary>
        public double ServedWatt { get; }

        /// <summary>充電量 [W]。ゲーム内表示の「充電工率」に対応する。</summary>
        public double ChargeWatt { get; }

        /// <summary>放電量 [W]。ゲーム内表示の「放電工率」に対応する。</summary>
        public double DischargeWatt { get; }

        /// <summary>差し引きの充放電 [W]。正なら充電超過、負なら放電超過。</summary>
        public double NetChargeWatt
        {
            get { return ChargeWatt - DischargeWatt; }
        }

        /// <summary>充足率（0.0〜1.0）。需要が 0 のときは 1.0。</summary>
        public double SatisfactionRatio { get; }

        /// <summary>使用率。実発電量 / 発電容量。容量が 0 のときは 0。</summary>
        public double UtilizationRatio { get; }

        /// <summary>蓄電池の蓄電量 [J]。tick 換算の対象外。</summary>
        public double StoredJoule { get; }

        /// <summary>集計対象が存在しないことを表すスナップショット。</summary>
        public static PowerSnapshot Invalid => default;
    }
}
