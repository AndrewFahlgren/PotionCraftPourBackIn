using HarmonyLib;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.InteractiveItem.InventoryObject;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    public static partial class UnfinishedPotionPatcher
    {
        [HarmonyPatch(typeof(PotionCraftPanel), "UpdatePotionInCraftPanel")]
        public class EnableFinishPotionButtonForUnfinishedPotionsPatch
        {
            static void Postfix(PotionCraftPanel __instance)
            {
                Ex.RunSafe(() => EnableFinishPotionButtonForUnfinishedPotions(__instance));
            }

            private static void EnableFinishPotionButtonForUnfinishedPotions(PotionCraftPanel instance)
            {
                if (!instance.IsPotionBrewingStarted()) return;
                var limitedInventoryPanelButtons = instance.limitedInventoryPanel.buttons;
                limitedInventoryPanelButtons.First().Locked = false;
                instance.potionFinishingButton.Locked = false;
            }
        }
    }
}
