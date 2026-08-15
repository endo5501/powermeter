using System.Collections.Generic;
using PowerMeter.Core;

namespace PowerMeter.Plugin
{
    /// <summary>
    /// ゲームの状態から電力網のサンプルを取り出す。
    /// PowerMeter でゲーム側の型に触れるのはこのクラスだけ。
    /// </summary>
    public static class GamePowerSampler
    {
        /// <summary>
        /// 全惑星の電力網を <paramref name="buffer"/> へ収集する。
        /// </summary>
        /// <returns>ゲームが稼働中でサンプリングできた場合に true。</returns>
        public static bool TryCollect(List<NetworkSample> buffer, out int localPlanetId, out int localStarId)
        {
            buffer.Clear();
            localPlanetId = 0;
            localStarId = 0;

            if (GameMain.instance == null || GameMain.instance.isMenuDemo || !GameMain.isRunning)
            {
                return false;
            }

            var data = GameMain.data;
            if (data == null || data.factories == null)
            {
                return false;
            }

            var localPlanet = GameMain.localPlanet;
            if (localPlanet != null)
            {
                localPlanetId = localPlanet.id;
            }

            var localStar = GameMain.localStar;
            if (localStar != null)
            {
                localStarId = localStar.id;
            }

            var factoryCount = data.factoryCount;
            if (factoryCount > data.factories.Length)
            {
                factoryCount = data.factories.Length;
            }

            for (var i = 0; i < factoryCount; i++)
            {
                var factory = data.factories[i];
                if (factory == null)
                {
                    continue;
                }

                var planet = factory.planet;
                var powerSystem = factory.powerSystem;
                if (planet == null || powerSystem == null || powerSystem.netPool == null)
                {
                    continue;
                }

                var planetId = planet.id;
                var starId = planet.star != null ? planet.star.id : 0;

                var netCursor = powerSystem.netCursor;
                if (netCursor > powerSystem.netPool.Length)
                {
                    netCursor = powerSystem.netPool.Length;
                }

                // netPool[0] は常に未使用。撤去済みの電力網は null か id == 0 で残るため読み飛ばす。
                for (var j = 1; j < netCursor; j++)
                {
                    var net = powerSystem.netPool[j];
                    if (net == null || net.id != j)
                    {
                        continue;
                    }

                    buffer.Add(new NetworkSample(
                        planetId,
                        starId,
                        net.energyCapacity,
                        net.energyRequired,
                        net.energyServed,
                        net.energyCharge,
                        net.energyDischarge,
                        net.energyAccumulated));
                }
            }

            return true;
        }

        /// <summary>1 秒あたりの tick 数。ゲーム側の設定を尊重する。</summary>
        public static int TickPerSecond
        {
            get
            {
                var tps = GameMain.tickPerSecI;
                return tps > 0 ? tps : 60;
            }
        }
    }
}
