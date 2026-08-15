namespace PowerMeter.Core
{
    /// <summary>
    /// 1 つの電力網（PowerNetwork）から読み取った生の値。
    /// エネルギー量はいずれも「1 tick あたり」のゲーム内部単位で、W ではない。
    /// </summary>
    public readonly struct NetworkSample
    {
        public NetworkSample(
            int planetId,
            int starId,
            long energyCapacity,
            long energyRequired,
            long energyServed,
            long energyCharge = 0L,
            long energyDischarge = 0L,
            long energyStored = 0L)
        {
            PlanetId = planetId;
            StarId = starId;
            EnergyCapacity = energyCapacity;
            EnergyRequired = energyRequired;
            EnergyServed = energyServed;
            EnergyCharge = energyCharge;
            EnergyDischarge = energyDischarge;
            EnergyStored = energyStored;
        }

        /// <summary>この電力網が属する惑星の ID。</summary>
        public int PlanetId { get; }

        /// <summary>この電力網が属する恒星の ID。</summary>
        public int StarId { get; }

        /// <summary>最大発電能力。</summary>
        public long EnergyCapacity { get; }

        /// <summary>消費側の需要合計。</summary>
        public long EnergyRequired { get; }

        /// <summary>実際に消費側へ供給された量。</summary>
        public long EnergyServed { get; }

        /// <summary>蓄電池へ充電された量。</summary>
        public long EnergyCharge { get; }

        /// <summary>蓄電池から放電された量。</summary>
        public long EnergyDischarge { get; }

        /// <summary>
        /// 蓄電池に蓄えられているエネルギー総量。
        /// ゲーム側の統計ウィンドウが「蓄電量」として使うのもこの値
        /// (<c>PowerNetwork.energyStored</c>)。よく似た名前の
        /// <c>energyAccumulated</c> は建物ツールチップ用の別物なので使わない。
        /// </summary>
        public long EnergyStored { get; }
    }
}
