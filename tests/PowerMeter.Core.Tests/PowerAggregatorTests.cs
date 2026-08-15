using System;
using System.Collections.Generic;
using Xunit;

namespace PowerMeter.Core.Tests
{
    public class PowerAggregatorTests
    {
        private const int TickPerSecond = 60;

        /// <summary>星系 1 の惑星 101/102、星系 2 の惑星 201 にまたがるサンプル群。</summary>
        private static List<NetworkSample> SampleSet()
        {
            return new List<NetworkSample>
            {
                //             planet star capacity required served
                new NetworkSample(101, 1, 100, 80, 80),
                new NetworkSample(101, 1, 50, 20, 20),
                new NetworkSample(102, 1, 400, 300, 250),
                new NetworkSample(201, 2, 1000, 10, 10),
            };
        }

        [Fact]
        public void 対象がひとつも無ければ無効なスナップショットを返す()
        {
            var result = PowerAggregator.Aggregate(
                new List<NetworkSample>(), PowerScope.Global, 0, 0, TickPerSecond);

            Assert.False(result.IsValid);
            Assert.Equal(0, result.NetworkCount);
            Assert.Equal(0.0, result.CapacityWatt);
            Assert.Equal(0.0, result.ConsumptionWatt);
        }

        [Fact]
        public void samplesがnullなら無効なスナップショットを返す()
        {
            var result = PowerAggregator.Aggregate(null, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void 惑星スコープは指定した惑星の電力網だけを合算する()
        {
            var result = PowerAggregator.Aggregate(
                SampleSet(), PowerScope.Planet, planetId: 102, starId: 1, tickPerSecond: TickPerSecond);

            Assert.True(result.IsValid);
            Assert.Equal(1, result.NetworkCount);
            Assert.Equal(400 * TickPerSecond, result.CapacityWatt);
            Assert.Equal(300 * TickPerSecond, result.ConsumptionWatt);
            Assert.Equal(250 * TickPerSecond, result.ServedWatt);
        }

        [Fact]
        public void 惑星スコープは同一惑星の複数電力網を合算する()
        {
            var result = PowerAggregator.Aggregate(
                SampleSet(), PowerScope.Planet, planetId: 101, starId: 1, tickPerSecond: TickPerSecond);

            Assert.True(result.IsValid);
            Assert.Equal(2, result.NetworkCount);
            Assert.Equal(150 * TickPerSecond, result.CapacityWatt);
            Assert.Equal(100 * TickPerSecond, result.ConsumptionWatt);
        }

        [Fact]
        public void 惑星スコープで該当する惑星が無ければ無効を返す()
        {
            var result = PowerAggregator.Aggregate(
                SampleSet(), PowerScope.Planet, planetId: 999, starId: 1, tickPerSecond: TickPerSecond);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void 星系スコープは配下の全惑星を合算する()
        {
            var result = PowerAggregator.Aggregate(
                SampleSet(), PowerScope.Star, planetId: 101, starId: 1, tickPerSecond: TickPerSecond);

            Assert.True(result.IsValid);
            Assert.Equal(3, result.NetworkCount);
            Assert.Equal(550 * TickPerSecond, result.CapacityWatt);
            Assert.Equal(400 * TickPerSecond, result.ConsumptionWatt);
            Assert.Equal(350 * TickPerSecond, result.ServedWatt);
        }

        [Fact]
        public void 星系スコープで該当する恒星が無ければ無効を返す()
        {
            var result = PowerAggregator.Aggregate(
                SampleSet(), PowerScope.Star, planetId: 0, starId: 99, tickPerSecond: TickPerSecond);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void 全星系スコープは全ての電力網を合算する()
        {
            var result = PowerAggregator.Aggregate(
                SampleSet(), PowerScope.Global, planetId: 0, starId: 0, tickPerSecond: TickPerSecond);

            Assert.True(result.IsValid);
            Assert.Equal(4, result.NetworkCount);
            Assert.Equal(1550 * TickPerSecond, result.CapacityWatt);
            Assert.Equal(410 * TickPerSecond, result.ConsumptionWatt);
            Assert.Equal(360 * TickPerSecond, result.ServedWatt);
        }

        [Fact]
        public void tick単位の値をワットへ換算する()
        {
            var samples = new[] { new NetworkSample(101, 1, 10, 10, 10) };

            var at30 = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, tickPerSecond: 30);
            var at60 = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, tickPerSecond: 60);

            Assert.Equal(300.0, at30.CapacityWatt);
            Assert.Equal(600.0, at60.CapacityWatt);
        }

        [Fact]
        public void 需要がゼロなら充足率は１００パーセント扱いになる()
        {
            var samples = new[] { new NetworkSample(101, 1, 100, 0, 0) };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.True(result.IsValid);
            Assert.Equal(1.0, result.SatisfactionRatio);
        }

        [Fact]
        public void 供給が需要に満たない場合の充足率を計算する()
        {
            var samples = new[] { new NetworkSample(101, 1, 400, 400, 300) };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.Equal(0.75, result.SatisfactionRatio, 6);
        }

        [Fact]
        public void 実発電量は供給と充電の合計から放電を引いた値になる()
        {
            // 供給 100、充電 30、放電 20 -> 発電は 110
            var samples = new[]
            {
                new NetworkSample(101, 1, 200, 100, 100, energyCharge: 30, energyDischarge: 20),
            };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.Equal(110 * TickPerSecond, result.GenerationWatt);
        }

        [Fact]
        public void 実発電量は負にならない()
        {
            // 放電だけで需要を賄っている状況
            var samples = new[]
            {
                new NetworkSample(101, 1, 0, 100, 100, energyCharge: 0, energyDischarge: 150),
            };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.Equal(0.0, result.GenerationWatt);
        }

        [Fact]
        public void 充電量と放電量をそれぞれ合算する()
        {
            var samples = new[]
            {
                new NetworkSample(101, 1, 0, 0, 0, energyCharge: 30, energyDischarge: 20),
                new NetworkSample(102, 1, 0, 0, 0, energyCharge: 5, energyDischarge: 1),
            };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.Equal(35 * TickPerSecond, result.ChargeWatt);
            Assert.Equal(21 * TickPerSecond, result.DischargeWatt);
        }

        [Fact]
        public void 差し引きの充放電は充電から放電を引いた値になる()
        {
            var samples = new[]
            {
                new NetworkSample(101, 1, 0, 0, 0, energyCharge: 30, energyDischarge: 20),
                new NetworkSample(102, 1, 0, 0, 0, energyCharge: 5, energyDischarge: 1),
            };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.Equal(14 * TickPerSecond, result.NetChargeWatt);
        }

        [Fact]
        public void 使用率は実発電量を発電容量で割った値になる()
        {
            var samples = new[] { new NetworkSample(101, 1, 200, 100, 100) };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.Equal(0.5, result.UtilizationRatio, 6);
        }

        [Fact]
        public void 発電容量がゼロなら使用率はゼロになる()
        {
            var samples = new[] { new NetworkSample(101, 1, 0, 100, 100, energyDischarge: 100) };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.Equal(0.0, result.UtilizationRatio);
        }

        // 以下 2 件は実機（電力融通器を使った惑星）で確認した挙動を固定するための回帰テスト。
        [Fact]
        public void 融通器から受電している惑星では実発電量がゼロになる()
        {
            // 現地の発電機はほぼ動かず、需要は放電で賄われている状態。
            var samples = new[]
            {
                new NetworkSample(101, 1, energyCapacity: 1683, energyRequired: 28666,
                    energyServed: 28666, energyCharge: 0, energyDischarge: 28666),
            };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.Equal(0.0, result.GenerationWatt);
            Assert.Equal(0.0, result.UtilizationRatio);
            Assert.Equal(-28666.0 * TickPerSecond, result.NetChargeWatt);
        }

        [Fact]
        public void 融通器へ充電している惑星では実発電量が需要と充電の合計になる()
        {
            var samples = new[]
            {
                new NetworkSample(101, 1, energyCapacity: 1500, energyRequired: 100,
                    energyServed: 100, energyCharge: 1200, energyDischarge: 0),
            };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.Equal(1300.0 * TickPerSecond, result.GenerationWatt);
            Assert.Equal(1300.0 / 1500.0, result.UtilizationRatio, 6);
            Assert.Equal(1200.0 * TickPerSecond, result.NetChargeWatt);
        }

        [Fact]
        public void 蓄電量はtick換算せずそのまま合算する()
        {
            var samples = new[]
            {
                new NetworkSample(101, 1, 0, 0, 0, energyStored: 1000),
                new NetworkSample(102, 1, 0, 0, 0, energyStored: 2000),
            };

            var result = PowerAggregator.Aggregate(samples, PowerScope.Global, 0, 0, TickPerSecond);

            Assert.Equal(3000.0, result.StoredJoule);
        }
    }
}
