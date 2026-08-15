using BepInEx;

namespace PowerMeter.Plugin
{
    /// <summary>PowerMeter の BepInEx エントリポイント。</summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class PowerMeterPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.endo5501.dsp.PowerMeter";
        public const string PluginName = "PowerMeter";
        public const string PluginVersion = "0.1.0";

        private void Awake()
        {
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }
    }
}
