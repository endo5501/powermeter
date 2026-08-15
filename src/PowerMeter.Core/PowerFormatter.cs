using System;
using System.Globalization;

namespace PowerMeter.Core
{
    /// <summary>電力値・割合の文字列整形。</summary>
    public static class PowerFormatter
    {
        private static readonly string[] Units = { "W", "kW", "MW", "GW", "TW", "PW", "EW" };

        /// <summary>
        /// W 値を有効数字 3 桁で単位付き整形する（例: "980 MW", "1.20 GW"）。
        /// </summary>
        public static string FormatWatt(double watt)
        {
            if (double.IsNaN(watt) || double.IsInfinity(watt))
            {
                return "- W";
            }

            var isNegative = watt < 0.0;
            var value = Math.Abs(watt);

            // 1 W 未満は意味のある表示にならないため 0 W に丸める。
            if (value < 1.0)
            {
                return "0 W";
            }

            var unitIndex = 0;
            while (value >= 1000.0 && unitIndex < Units.Length - 1)
            {
                value /= 1000.0;
                unitIndex++;
            }

            var decimals = DecimalsFor(value);
            var rounded = Math.Round(value, decimals, MidpointRounding.AwayFromZero);

            // 丸めた結果 1000 に達したら 1 段上の単位へ繰り上げる（例: 999.6 W -> 1.00 kW）。
            if (rounded >= 1000.0 && unitIndex < Units.Length - 1)
            {
                unitIndex++;
                rounded /= 1000.0;
                decimals = DecimalsFor(rounded);
                rounded = Math.Round(rounded, decimals, MidpointRounding.AwayFromZero);
            }

            var sign = isNegative ? "-" : string.Empty;
            return sign
                + rounded.ToString("F" + decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
                + " "
                + Units[unitIndex];
        }

        /// <summary>
        /// 充足率（0.0〜1.0）を整数パーセントで整形する（例: "89%"）。
        /// 1.0 未満は切り捨てるため、需要が満たされていない限り "100%" にはならない。
        /// </summary>
        public static string FormatPercent(double ratio)
        {
            if (double.IsNaN(ratio) || ratio <= 0.0)
            {
                return "0%";
            }

            if (ratio >= 1.0)
            {
                return "100%";
            }

            var percent = (int)Math.Floor(ratio * 100.0);
            if (percent > 99)
            {
                percent = 99;
            }

            return percent.ToString(CultureInfo.InvariantCulture) + "%";
        }

        /// <summary>仮数部が有効数字 3 桁になる小数桁数を返す。</summary>
        private static int DecimalsFor(double mantissa)
        {
            if (mantissa >= 100.0)
            {
                return 0;
            }

            return mantissa >= 10.0 ? 1 : 2;
        }
    }
}
