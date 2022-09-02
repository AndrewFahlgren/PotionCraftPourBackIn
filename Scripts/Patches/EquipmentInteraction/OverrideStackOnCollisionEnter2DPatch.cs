using PotionCraft.ObjectBased.Stack;
using System.Linq;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.UIElements;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(Stack), "OnCollisionEnter2D")]
    public class OverrideStackOnCollisionEnter2DPatch
    {
        static bool Prefix(Stack __instance)
        {
            return Ex.RunSafe(() => OverrideStackOnCollisionEnter2D(__instance));
        }

        private static bool OverrideStackOnCollisionEnter2D(Stack instance)
        {
            if (instance.itemsFromThisStack.FirstOrDefault() is PotionStackItem) return false;
            return true;
        }
    }
}
