using System;
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
            throw new NotImplementedException();
        }
    }
}
