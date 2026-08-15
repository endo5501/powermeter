using System;
using System.Globalization;

namespace PowerMeter.Core
{
    /// <summary>電力値・エネルギー値・割合の文字列整形。</summary>
    public static class PowerFormatter
    {
        private static readonly string[] WattUnits = { "W", "kW", "MW", "GW", "TW", "PW", "EW" };
        private static readonly string[] JouleUnits = { "J", "kJ", "MJ", "GJ", "TJ", "PJ", "EJ" };

        /// <summary>
        /// W 値を有効数字 3 桁で単位付き整形する（例: "980 MW", "1.20 GW"）。
        /// </summary>
        public static string FormatWatt(double watt)
        {
            return Format(watt, WattUnits, false);
        }

        /// <summary>
        /// 符号付きで W 値を整形する（例: "+27.3 GW", "-1.72 GW"）。
        /// 充放電のように向きが意味を持つ値に使う。
        /// </summary>
        public static string FormatSignedWatt(double watt)
        {
            return Format(watt, WattUnits, true);
        }

        /// <summary>
        /// J 値を有効数字 3 桁で単位付き整形する（例: "21.0 GJ", "520 TJ"）。
        /// </summary>
        public static string FormatJoule(double joule)
        {
            return Format(joule, JouleUnits, false);
        }

        /// <summary>
        /// 割合（0.0〜1.0）を整数パーセントで整形する（例: "89%"）。
        /// 1.0 未満は切り捨てるため、満たされていない限り "100%" にはならない。
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

        private static string Format(double value, string[] units, bool alwaysSigned)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return "- " + units[0];
            }

            var isNegative = value < 0.0;
            var magnitude = Math.Abs(value);

            // 最小単位で 1 に満たない値は意味のある表示にならないため 0 に丸める。
            if (magnitude < 1.0)
            {
                return "0 " + units[0];
            }

            var unitIndex = 0;
            while (magnitude >= 1000.0 && unitIndex < units.Length - 1)
            {
                magnitude /= 1000.0;
                unitIndex++;
            }

            var decimals = DecimalsFor(magnitude);
            var rounded = Math.Round(magnitude, decimals, MidpointRounding.AwayFromZero);

            // 丸めた結果 1000 に達したら 1 段上の単位へ繰り上げる（例: 999.6 W -> 1.00 kW）。
            if (rounded >= 1000.0 && unitIndex < units.Length - 1)
            {
                unitIndex++;
                rounded /= 1000.0;
                decimals = DecimalsFor(rounded);
                rounded = Math.Round(rounded, decimals, MidpointRounding.AwayFromZero);
            }

            string sign;
            if (isNegative)
            {
                sign = "-";
            }
            else
            {
                sign = alwaysSigned ? "+" : string.Empty;
            }

            return sign
                + rounded.ToString("F" + decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
                + " "
                + units[unitIndex];
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
