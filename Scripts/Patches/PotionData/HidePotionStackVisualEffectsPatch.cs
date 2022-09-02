using HarmonyLib;
using PotionCraft.ObjectBased.Stack;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(StackVisualEffects), "SpawnEffectsUpdate")]
    public class HidePotionStackVisualEffectsPatch
    {
        static bool Prefix(StackVisualEffects __instance)
        {
            return Ex.RunSafe(() => !PotionDataService.IsPotionStackItemEffect(__instance));
        }
    }
}
