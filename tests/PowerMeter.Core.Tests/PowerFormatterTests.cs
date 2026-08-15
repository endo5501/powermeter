using Xunit;

namespace PowerMeter.Core.Tests
{
    public class PowerFormatterTests
    {
        [Theory]
        [InlineData(0.0, "0 W")]
        [InlineData(0.4, "0 W")]          // 1 W 未満は 0 W とする
        [InlineData(1.0, "1.00 W")]
        [InlineData(12.3, "12.3 W")]
        [InlineData(999.0, "999 W")]
        [InlineData(1000.0, "1.00 kW")]
        [InlineData(1500.0, "1.50 kW")]
        [InlineData(980_000_000.0, "980 MW")]
        [InlineData(1_200_000_000.0, "1.20 GW")]
        [InlineData(12_400_000_000.0, "12.4 GW")]
        [InlineData(1_000_000_000_000.0, "1.00 TW")]
        public void ワット値を有効数字３桁で単位付き整形する(double watt, string expected)
        {
            Assert.Equal(expected, PowerFormatter.FormatWatt(watt));
        }

        [Theory]
        [InlineData(999.6, "1.00 kW")]      // 丸め上がりで単位が繰り上がる
        [InlineData(999_999.0, "1.00 MW")]
        public void 丸めで桁が繰り上がる場合は単位も繰り上げる(double watt, string expected)
        {
            Assert.Equal(expected, PowerFormatter.FormatWatt(watt));
        }

        [Theory]
        [InlineData(-1500.0, "-1.50 kW")]
        [InlineData(-0.4, "0 W")]
        public void 負の値も整形できる(double watt, string expected)
        {
            Assert.Equal(expected, PowerFormatter.FormatWatt(watt));
        }

        [Theory]
        [InlineData(1.0, "100%")]
        [InlineData(1.5, "100%")]      // 1.0 を超えても 100% に丸める
        [InlineData(0.891, "89%")]
        [InlineData(0.999, "99%")]     // 満たしていないのに 100% と表示しない
        [InlineData(0.0, "0%")]
        [InlineData(-0.2, "0%")]
        public void 充足率を整数パーセントで整形する(double ratio, string expected)
        {
            Assert.Equal(expected, PowerFormatter.FormatPercent(ratio));
        }
    }
}
