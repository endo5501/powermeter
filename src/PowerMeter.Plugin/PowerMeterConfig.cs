using System;
using BepInEx.Configuration;
using UnityEngine;

namespace PowerMeter.Plugin
{
    /// <summary>ウィジットを配置する画面の隅。</summary>
    public enum WidgetCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    /// <summary>ウィジットの表示言語。</summary>
    public enum WidgetLanguage
    {
        /// <summary>ゲームの言語設定に従う。</summary>
        Auto,
        Japanese,
        English,
    }

    /// <summary>充放電列の出し方。</summary>
    public enum ChargeColumnMode
    {
        /// <summary>表示しない。</summary>
        Off,

        /// <summary>充電 - 放電 の差し引きを 1 列で符号付き表示する。</summary>
        Net,

        /// <summary>充電と放電を別々の列に表示する（ゲーム内パネルと同じ形）。</summary>
        Split,
    }

    /// <summary>BepInEx 設定ファイルのバインディング。</summary>
    public class PowerMeterConfig
    {
        public PowerMeterConfig(ConfigFile file)
        {
            File = file;

            Enabled = file.Bind(
                "General", "Enabled", true,
                "PowerMeter を有効にする。");

            ToggleHotkey = file.Bind(
                "General", "ToggleHotkey", new KeyboardShortcut(KeyCode.P, KeyCode.LeftAlt),
                "ウィジットの表示を切り替えるキー。");

            UpdateIntervalSeconds = file.Bind(
                "General", "UpdateIntervalSeconds", 0.5f,
                new ConfigDescription(
                    "電力値を再集計する間隔（秒）。短くすると追従は良くなるが負荷が上がる。",
                    new AcceptableValueRange<float>(0.1f, 5.0f)));

            Language = file.Bind(
                "General", "Language", WidgetLanguage.Auto,
                "ウィジットのラベル言語。Auto はゲームの言語設定に従う。");

            Corner = file.Bind(
                "Layout", "Corner", WidgetCorner.TopRight,
                "ウィジットを表示する画面の隅。");

            OffsetX = file.Bind(
                "Layout", "OffsetX", 16f,
                new ConfigDescription(
                    "指定した隅からの横方向のオフセット。",
                    new AcceptableValueRange<float>(0f, 2000f)));

            OffsetY = file.Bind(
                "Layout", "OffsetY", 16f,
                new ConfigDescription(
                    "指定した隅からの縦方向のオフセット。",
                    new AcceptableValueRange<float>(0f, 2000f)));

            FontSize = file.Bind(
                "Layout", "FontSize", 14,
                new ConfigDescription(
                    "文字サイズ。ウィジット全体の寸法もこれに追従する。",
                    new AcceptableValueRange<int>(8, 32)));

            BackgroundOpacity = file.Bind(
                "Layout", "BackgroundOpacity", 0.55f,
                new ConfigDescription(
                    "背景パネルの不透明度。0 で背景なし。",
                    new AcceptableValueRange<float>(0f, 1f)));

            ShowCapacity = file.Bind(
                "Columns", "ShowCapacity", true,
                "発電容量（最大発電能力）の列を表示する。ゲーム内の「発電性能」に対応する。");

            ShowUtilization = file.Bind(
                "Columns", "ShowUtilization", true,
                "使用率（実発電量 / 発電容量）の列を表示する。発電設備の余力を見るための指標。");

            ShowSatisfaction = file.Bind(
                "Columns", "ShowSatisfaction", false,
                "充足率（供給 / 需要）の列を表示する。電力不足のときだけ 100% を下回る。");

            ChargeColumn = file.Bind(
                "Columns", "ChargeColumn", ChargeColumnMode.Split,
                "充放電の列の出し方。Split はゲーム内パネルと同じく充電と放電を分けて表示し、"
                + "Net は差し引きを 1 列にまとめる。");

            ShowAccumulated = file.Bind(
                "Columns", "ShowAccumulated", false,
                "蓄電量（蓄電池に貯まっているエネルギー）の列を表示する。");

            UtilizationWarningPercent = file.Bind(
                "Columns", "UtilizationWarningPercent", 90,
                new ConfigDescription(
                    "使用率がこの値以上になったら警告色で表示する。発電設備の増設時期の目安。",
                    new AcceptableValueRange<int>(0, 100)));

            SatisfactionWarningPercent = file.Bind(
                "Columns", "SatisfactionWarningPercent", 95,
                new ConfigDescription(
                    "充足率がこの値を下回ったら警告色で表示する。",
                    new AcceptableValueRange<int>(0, 100)));

            // 数値の突き合わせが済むまでは既定で有効にしておく。
            DiagnosticLogging = file.Bind(
                "Diagnostics", "DiagnosticLogging", true,
                "集計結果を BepInEx のログへ定期出力する。ゲーム内統計ウィンドウとの突き合わせ用。");

            DiagnosticLogIntervalSeconds = file.Bind(
                "Diagnostics", "DiagnosticLogIntervalSeconds", 5.0f,
                new ConfigDescription(
                    "診断ログを出力する間隔（秒）。",
                    new AcceptableValueRange<float>(1.0f, 60.0f)));
        }

        public ConfigFile File { get; }

        public ConfigEntry<bool> Enabled { get; }

        public ConfigEntry<KeyboardShortcut> ToggleHotkey { get; }

        public ConfigEntry<float> UpdateIntervalSeconds { get; }

        public ConfigEntry<WidgetLanguage> Language { get; }

        public ConfigEntry<WidgetCorner> Corner { get; }

        public ConfigEntry<float> OffsetX { get; }

        public ConfigEntry<float> OffsetY { get; }

        public ConfigEntry<int> FontSize { get; }

        public ConfigEntry<float> BackgroundOpacity { get; }

        public ConfigEntry<bool> ShowCapacity { get; }

        public ConfigEntry<bool> ShowUtilization { get; }

        public ConfigEntry<bool> ShowSatisfaction { get; }

        public ConfigEntry<ChargeColumnMode> ChargeColumn { get; }

        public ConfigEntry<bool> ShowAccumulated { get; }

        public ConfigEntry<int> UtilizationWarningPercent { get; }

        public ConfigEntry<int> SatisfactionWarningPercent { get; }

        public ConfigEntry<bool> DiagnosticLogging { get; }

        public ConfigEntry<float> DiagnosticLogIntervalSeconds { get; }

        /// <summary>Auto を解決した実際の表示言語が日本語かどうか。</summary>
        public bool UseJapanese
        {
            get
            {
                switch (Language.Value)
                {
                    case WidgetLanguage.Japanese:
                        return true;
                    case WidgetLanguage.English:
                        return false;
                    default:
                        return IsGameJapanese();
                }
            }
        }

        private static bool IsGameJapanese()
        {
            try
            {
                return Localization.isJAJA;
            }
            catch (Exception)
            {
                // 言語データ未ロードのタイミングでは英語にフォールバックする。
                return false;
            }
        }
    }
}
