using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using HarmonyLib;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(PotionItem), "VacuumItem")]
    public class OverridePotionItemVaccumItemPatch
    {
        static bool Prefix(PotionItem __instance, bool isPrimaryGrab, bool forceMassModifier, bool forceAltModifier)
        {
            return Ex.RunSafe(() => OverridePotionItemVaccumItem(__instance, isPrimaryGrab, forceMassModifier, forceAltModifier));
        }

        private static bool OverridePotionItemVaccumItem(PotionItem instance, bool isPrimaryGrab, bool forceMassModifier, bool forceAltModifier)
        {
            if (!isPrimaryGrab) return true;

            var stack = instance.gameObject.GetComponent<Stack>(); ;
            stack.VacuumItem(isPrimaryGrab, forceMassModifier, forceAltModifier);

            return false;
        }
    }
}
