using System;
using System.Collections.Generic;
using BepInEx;
using PowerMeter.Core;
using UnityEngine;

namespace PowerMeter.Plugin
{
    /// <summary>PowerMeter の BepInEx エントリポイント。</summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class PowerMeterPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.endo5501.dsp.PowerMeter";
        public const string PluginName = "PowerMeter";
        public const string PluginVersion = "0.1.0";

        private readonly List<NetworkSample> _samples = new List<NetworkSample>(256);

        private PowerMeterConfig _config;
        private float _sinceRefresh;
        private float _sinceDiagnosticLog;
        private bool _disabledByError;

        /// <summary>直近の集計結果。現在の惑星。</summary>
        public PowerSnapshot Planet { get; private set; }

        /// <summary>直近の集計結果。現在の星系。</summary>
        public PowerSnapshot Star { get; private set; }

        /// <summary>直近の集計結果。全星系。</summary>
        public PowerSnapshot Global { get; private set; }

        private void Awake()
        {
            _config = new PowerMeterConfig(Config);
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        private void Update()
        {
            if (_disabledByError || _config == null || !_config.Enabled.Value)
            {
                return;
            }

            var delta = Time.unscaledDeltaTime;
            _sinceRefresh += delta;
            _sinceDiagnosticLog += delta;

            if (_sinceRefresh < _config.UpdateIntervalSeconds.Value)
            {
                return;
            }

            _sinceRefresh = 0f;

            try
            {
                Refresh();
            }
            catch (Exception e)
            {
                _disabledByError = true;
                Logger.LogError($"{PluginName} を無効化しました（更新中に例外が発生）: {e}");
            }
        }

        private void Refresh()
        {
            if (!GamePowerSampler.TryCollect(_samples, out var planetId, out var starId))
            {
                Planet = PowerSnapshot.Invalid;
                Star = PowerSnapshot.Invalid;
                Global = PowerSnapshot.Invalid;
                return;
            }

            var tps = GamePowerSampler.TickPerSecond;
            Planet = PowerAggregator.Aggregate(_samples, PowerScope.Planet, planetId, starId, tps);
            Star = PowerAggregator.Aggregate(_samples, PowerScope.Star, planetId, starId, tps);
            Global = PowerAggregator.Aggregate(_samples, PowerScope.Global, planetId, starId, tps);

            if (_config.DiagnosticLogging.Value
                && _sinceDiagnosticLog >= _config.DiagnosticLogIntervalSeconds.Value)
            {
                _sinceDiagnosticLog = 0f;
                LogDiagnostics(planetId, starId, tps);
            }
        }

        private void LogDiagnostics(int planetId, int starId, int tickPerSecond)
        {
            Logger.LogInfo(
                $"[diag] tps={tickPerSecond} planetId={planetId} starId={starId} networks={_samples.Count}");
            Logger.LogInfo($"[diag] 惑星   {Describe(Planet)}");
            Logger.LogInfo($"[diag] 星系   {Describe(Star)}");
            Logger.LogInfo($"[diag] 全星系 {Describe(Global)}");
        }

        private static string Describe(PowerSnapshot s)
        {
            if (!s.IsValid)
            {
                return "(対象なし)";
            }

            return $"発電 {PowerFormatter.FormatWatt(s.GenerationWatt)}"
                + $" / 需要 {PowerFormatter.FormatWatt(s.ConsumptionWatt)}"
                + $" / 供給 {PowerFormatter.FormatWatt(s.ServedWatt)}"
                + $" / 容量 {PowerFormatter.FormatWatt(s.CapacityWatt)}"
                + $" / 充足 {PowerFormatter.FormatPercent(s.SatisfactionRatio)}"
                + $" / 網数 {s.NetworkCount}";
        }
    }
}
