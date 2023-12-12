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
using PotionCraft.ObjectBased.UIElements.Books;

namespace PotionCraftPourBackIn.Scripts.Patches.UnfinishedPotion
{
    public class AllowBrewingNoEffectPotionsFromRecipeBookPatch
    {
        [HarmonyPatch(typeof(RecipeBookPotionPlaceholderForTooltip), "GetTooltipContent")]
        public class RecipeBookPotionPlaceholderForTooltip_GetTooltipContent
        {
            static void Postfix(ref TooltipContent __result, RecipeBookLeftPageContent ___leftPageContent)
            {
                OverrideRecipeBookPotionIconTooltip(ref __result, ___leftPageContent);
            }
        }

        [HarmonyPatch(typeof(RecipeBookPotionInventoryObject), "UpdateVisual")]
        public class RecipeBookPotionInventoryObject_UpdateVisual
        {
            static void Postfix(RecipeBookPotionInventoryObject __instance)
            {
                Ex.RunSafe(() => AllowBrewingNoEffectPotionsFromRecipeBook(__instance));
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

        private static void AllowBrewingNoEffectPotionsFromRecipeBook(RecipeBookPotionInventoryObject instance)
        {
            //Only proceed if the inventory object is actually locked
            if (!instance.Locked) return;

            var inventoryPanel = instance.itemsPanel as RecipeBookPanelInventoryPanel;
            var pageState = inventoryPanel.leftPageContent.currentState;
            var currentPotion = inventoryPanel.leftPageContent.pageContentPotion;

            //Do not unlock the potion slot for an empty page
            if (pageState == PageContent.State.Empty) return;

            //Only proceed if this is a no effect potion
            var isEmptyPotion = currentPotion.Effects.Length == 0 || currentPotion.Effects[0] == null;
            if (!isEmptyPotion) return;

            //If this potion does not have enough ingredients to be brewed then keep it locked.
            var availBrewCount = RecipeBook.GetAvailableResultPotionsCount(pageState, currentPotion);
            if (availBrewCount == 0) return;
            
            //If there are enough ingredients to brew the no effect potion unlock it
            instance.Locked = false;
        }
    }
}
