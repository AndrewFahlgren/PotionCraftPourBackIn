using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace PotionCraftPourBackIn
{
    [BepInPlugin(PLUGIN_GUID, "PotionCraftPourBackIn", "0.5.0")]
    [BepInProcess("Potion Craft.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.fahlgorithm.potioncraftpourbackin";

        public static ManualLogSource PluginLogger {get; private set; }

        private void Awake()
        {
            PluginLogger = Logger;
            PluginLogger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);
            PluginLogger.LogInfo($"Plugin {PLUGIN_GUID}: Patch Succeeded!");
        }
    }
}
