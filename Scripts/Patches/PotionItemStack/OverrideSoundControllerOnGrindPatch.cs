using System.Reflection;
using SoundController = PotionCraft.ObjectBased.Stack.StackItem.SoundController;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.UIElements;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(SoundController), "OnGrind")]
    public class OverrideSoundControllerOnGrindPatch
    {
        static bool Prefix(SoundController __instance)
        {
            return Ex.RunSafe(() => OverrideSoundControllerOnGrind(__instance));
        }

        /// <summary>
        /// This should never be called for a PotionStackItem sound controller but it is better to override this method anyways to prevent exceptions should someone manage to grind a potion
        /// </summary>
        private static bool OverrideSoundControllerOnGrind(SoundController instance)
        {
            var stackItem = typeof(SoundController).GetField("stackItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
            return stackItem is not PotionStackItem;
        }
    }
}
