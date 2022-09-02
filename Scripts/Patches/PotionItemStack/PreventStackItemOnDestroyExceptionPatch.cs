using PotionCraft.ObjectBased.Stack.StackItem;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.UIElements;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(StackItem), "OnDestroy")]
    public class PreventStackItemOnDestroyExceptionPatch
    {
        static bool Prefix(StackItem __instance)
        {
            return Ex.RunSafe(() => PreventStackItemOnDestroyException(__instance));
        }

        private static bool PreventStackItemOnDestroyException(StackItem instance)
        {
            if (instance is not PotionStackItem) return true;
            instance.stackScript?.RemoveItemFromStack(instance);
            instance.soundController?.OnDestroy();
            return false;
        }
    }
}
