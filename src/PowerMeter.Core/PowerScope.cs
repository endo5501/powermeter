namespace PowerMeter.Core
{
    /// <summary>集計対象の範囲。</summary>
    public enum PowerScope
    {
        /// <summary>現在の惑星。</summary>
        Planet,

        /// <summary>現在の星系（配下の全惑星）。</summary>
        Star,

        /// <summary>全星系。</summary>
        Global,
    }
}
