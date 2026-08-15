using System;

namespace PowerMeter.Core
{
    /// <summary>電力値・割合の文字列整形。</summary>
    public static class PowerFormatter
    {
        /// <summary>
        /// W 値を有効数字 3 桁で単位付き整形する（例: "980 MW", "1.20 GW"）。
        /// </summary>
        public static string FormatWatt(double watt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 充足率（0.0〜1.0）を整数パーセントで整形する（例: "89%"）。
        /// 1.0 未満は切り捨てるため、needs が満たされていない限り "100%" にはならない。
        /// </summary>
        public static string FormatPercent(double ratio)
        {
            throw new NotImplementedException();
        }
    }
}
