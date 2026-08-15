using System.Collections.Generic;

namespace PowerMeter.Core
{
    /// <summary>電力網のサンプルをスコープ別に集計する。</summary>
    public static class PowerAggregator
    {
        /// <summary>
        /// 指定スコープに属する電力網を合算し、W 換算した結果を返す。
        /// </summary>
        /// <param name="samples">全電力網のサンプル。null 可。</param>
        /// <param name="scope">集計範囲。</param>
        /// <param name="planetId"><see cref="PowerScope.Planet"/> のときの対象惑星 ID。不明な場合は 0。</param>
        /// <param name="starId"><see cref="PowerScope.Star"/> のときの対象恒星 ID。不明な場合は 0。</param>
        /// <param name="tickPerSecond">1 秒あたりの tick 数（ゲームでは 60）。</param>
        public static PowerSnapshot Aggregate(
            IEnumerable<NetworkSample> samples,
            PowerScope scope,
            int planetId,
            int starId,
            int tickPerSecond)
        {
            if (samples == null)
            {
                return PowerSnapshot.Invalid;
            }

            var count = 0;
            long capacity = 0L;
            long required = 0L;
            long served = 0L;
            long charge = 0L;
            long discharge = 0L;
            long accumulated = 0L;

            foreach (var sample in samples)
            {
                if (!IsInScope(sample, scope, planetId, starId))
                {
                    continue;
                }

                count++;
                capacity += sample.EnergyCapacity;
                required += sample.EnergyRequired;
                served += sample.EnergyServed;
                charge += sample.EnergyCharge;
                discharge += sample.EnergyDischarge;
                accumulated += sample.EnergyAccumulated;
            }

            if (count == 0)
            {
                return PowerSnapshot.Invalid;
            }

            // 実発電量 = 消費側へ供給した分 + 蓄電池へ充電した分 - 蓄電池から放電された分。
            var generationPerTick = (double)(served + charge - discharge);
            if (generationPerTick < 0.0)
            {
                generationPerTick = 0.0;
            }

            // 需要が 0 の電力網（消費者がいない）は「満たされている」とみなす。
            var satisfaction = required > 0L ? (double)served / required : 1.0;

            // 発電設備が無い（融通器からの受電だけで動いている）電力網は使用率 0。
            var utilization = capacity > 0L ? generationPerTick / capacity : 0.0;

            return new PowerSnapshot(
                isValid: true,
                networkCount: count,
                capacityWatt: capacity * (double)tickPerSecond,
                generationWatt: generationPerTick * tickPerSecond,
                consumptionWatt: required * (double)tickPerSecond,
                servedWatt: served * (double)tickPerSecond,
                chargeWatt: charge * (double)tickPerSecond,
                dischargeWatt: discharge * (double)tickPerSecond,
                satisfactionRatio: satisfaction,
                utilizationRatio: utilization,
                accumulatedJoule: accumulated);
        }

        private static bool IsInScope(NetworkSample sample, PowerScope scope, int planetId, int starId)
        {
            switch (scope)
            {
                case PowerScope.Planet:
                    return sample.PlanetId == planetId;
                case PowerScope.Star:
                    return sample.StarId == starId;
                default:
                    return true;
            }
        }
    }
}
