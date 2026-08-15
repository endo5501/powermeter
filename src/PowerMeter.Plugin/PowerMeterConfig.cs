using BepInEx.Configuration;

namespace PowerMeter.Plugin
{
    /// <summary>BepInEx 設定ファイルのバインディング。</summary>
    public class PowerMeterConfig
    {
        public PowerMeterConfig(ConfigFile file)
        {
            Enabled = file.Bind(
                "General", "Enabled", true,
                "PowerMeter を有効にする。");

            UpdateIntervalSeconds = file.Bind(
                "General", "UpdateIntervalSeconds", 0.5f,
                new ConfigDescription(
                    "電力値を再集計する間隔（秒）。短くすると追従は良くなるが負荷が上がる。",
                    new AcceptableValueRange<float>(0.1f, 5.0f)));

            DiagnosticLogging = file.Bind(
                "Diagnostics", "DiagnosticLogging", true,
                "集計結果を BepInEx のログへ定期出力する。ゲーム内統計ウィンドウとの突き合わせ用。");

            DiagnosticLogIntervalSeconds = file.Bind(
                "Diagnostics", "DiagnosticLogIntervalSeconds", 5.0f,
                new ConfigDescription(
                    "診断ログを出力する間隔（秒）。",
                    new AcceptableValueRange<float>(1.0f, 60.0f)));
        }

        public ConfigEntry<bool> Enabled { get; }

        public ConfigEntry<float> UpdateIntervalSeconds { get; }

        public ConfigEntry<bool> DiagnosticLogging { get; }

        public ConfigEntry<float> DiagnosticLogIntervalSeconds { get; }
    }
}
