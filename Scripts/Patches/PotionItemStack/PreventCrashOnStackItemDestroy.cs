using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.Services;
using PotionCraft.ObjectBased.Stack.StackItem;
using PotionCraftPourBackIn.Scripts.UIElements;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    public class PreventCrashOnStackItemDestroy
    {
        [HarmonyPatch(typeof(StackItem), "RemoveItemFromStackAndReset")]
        public class StackItem_RemoveItemFromStackAndReset
        {
            static bool Prefix(StackItem __instance)
            {
                return Ex.RunSafe(() => DoPreventCrashOnStackItemDestroy(__instance));
            }
        }

        private static bool DoPreventCrashOnStackItemDestroy(StackItem instance)
        {
            if (instance is not PotionStackItem) return true;
            instance.stackScript?.RemoveItemFromStack(instance);
            instance.soundController.ResetController();
            return false;
        }
    }
}
