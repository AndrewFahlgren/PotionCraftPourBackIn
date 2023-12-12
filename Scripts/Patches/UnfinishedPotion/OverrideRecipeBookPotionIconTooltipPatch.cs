using HarmonyLib;
using PotionCraft.ManagersSystem.Input.ControlProviders;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.Cauldron;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack.StackItem;
using PotionCraft.ObjectBased.UIElements.Tooltip;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraftPourBackIn.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Text;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.LocalizationSystem;

namespace PotionCraftPourBackIn.Scripts.Patches.UnfinishedPotion
{
    public class OverrideRecipeBookPotionIconTooltipPatch
    {
        [HarmonyPatch(typeof(RecipeBookPotionPlaceholderForTooltip), "GetTooltipContent")]
        public class RecipeBookPotionPlaceholderForTooltip_GetTooltipContent
        {
            static void Postfix(ref TooltipContent __result, RecipeBookLeftPageContent ___leftPageContent)
            {
                OverrideRecipeBookPotionIconTooltip(ref __result, ___leftPageContent);
            }
        }

        private static void OverrideRecipeBookPotionIconTooltip(ref TooltipContent result, RecipeBookLeftPageContent leftPageContent)
        {
            if (result == null) return;
            var newResult = result;
            var header = result.header;
            Ex.RunSafe(() =>
            {
                //Only override the tooltip if it the no effects tooltip
                var noEffectToolTip = new Key("#recipebook_brew_potion_no_effects").GetText();
                if (header != noEffectToolTip) return;

                //As long as there are enough ingredients return a null tooltip
                if (RecipeBook.GetAvailableResultPotionsCount(leftPageContent.currentState, leftPageContent.pageContentPotion) != 0)
                {
                    newResult = null;
                    return;
                }

                //If there are not enough ingredients show the not enough ingredients tooltip
                var notEnoughIngredientsToolTip = new Key("#recipebook_brew_potion_not_enough_ingredients").GetText();
                newResult = new TooltipContent()
                {
                    header = notEnoughIngredientsToolTip
                };
            });
            result = newResult;
        }
    }
}
