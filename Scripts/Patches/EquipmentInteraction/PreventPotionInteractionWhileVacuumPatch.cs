using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using UnityEngine.Rendering;
using PotionCraft;
using HarmonyLib;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(PotionItem), "CanBeInteractedNow")]
    public class PreventPotionInteractionWhileVacuumPatch
    {
        static void Postfix(ref bool __result, PotionItem __instance)
        {
            PreventPotionInteractionWhileVacuum(ref __result, __instance);
        }

        private static void PreventPotionInteractionWhileVacuum(ref bool result, PotionItem instance)
        {
            if (result == false) return;
            var newResult = true;
            Ex.RunSafe(() =>
            {
                var stack = instance.GetComponent<Stack>();
                if (stack == null) return;
                newResult = stack.vacuumingTo == null && instance.GetComponent<SortingGroup>().sortingLayerID != (int)SpriteSortingLayers.IngredientsInCauldron;
            });
            if (!newResult) result = false;
        }
    }
}
