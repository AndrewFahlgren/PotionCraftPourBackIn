using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack.StackItem;
using PotionCraft;
using HarmonyLib;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(GraphicStateMachine), "SetCurrentGraphicState")]
    public class RemovePotionHighlightWhenEnteringCauldronPatch
    {
        static void Postfix(GraphicStateMachine __instance)
        {
            Ex.RunSafe(() => RemovePotionHighlightWhenEnteringCauldron(__instance));
        }
        private static void RemovePotionHighlightWhenEnteringCauldron(GraphicStateMachine instance)
        {
            var stackItem = instance.stackItem;
            if (stackItem.sortingGroup.sortingLayerName == SpriteSortingLayers.IngredientsInCauldron.ToString())
            {
                var stackHighlight = stackItem.stackScript.iHoverableScript as PotionItem;
                if (stackHighlight != null)
                {
                    stackHighlight.SetHovered(hover: false);
                }
            }
        }
    }
}
