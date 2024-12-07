using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ScriptableObjects;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraftPourBackIn.Scripts.Services;
using PotionCraftPourBackIn.Scripts.Storage;
using PotionCraftPourBackIn.Scripts.UIElements;
using System;
using System.Linq;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(Stack), "AddIngredientPathToMapPath")]
    public class OverrideAddIngredientPathToMapPathPatch
    {
        static bool Prefix(Stack __instance)
        {
            return Ex.RunSafe(() => OverrideAddIngredientPathToMapPath(__instance), () => OnError(__instance));
        }

        private static bool OnError(Stack stack)
        {
            try
            {
                if (stack.itemsFromThisStack.FirstOrDefault() is PotionStackItem) return false;
            }
            catch (Exception ex)
            {
                Ex.LogException(ex);
            }
            return true;
        }

        private static bool OverrideAddIngredientPathToMapPath(Stack instance)
        {
            var potionStackItem = instance.itemsFromThisStack.FirstOrDefault() as PotionStackItem ?? instance.vacuumedItemsFromThisStack.FirstOrDefault() as PotionStackItem;
            if (potionStackItem == null) return true;
            var potion = (Potion)Traverse.Create(potionStackItem.potionItem).Property<InventoryItem>("InventoryItem").Value;
            ContinueBrewingFromPotion(potion);
            return false;
        }

        private static void ContinueBrewingFromPotion(Potion potion)
        {
            var matchingRecipe = RecipeService.GetRecipeForPotion(potion);
            var potionFromPanel = !PotionDataService.PotionHasSerializedData(potion)
                                    ? matchingRecipe
                                    : (SerializedPotionRecipeData)potion.GetSerializedRecipeData();
            if (potionFromPanel == null)
            {
                Plugin.PluginLogger.LogInfo("ERROR: Failed to find recipe for poured-in, pre-mod potion!");
                return;
            }
            var potionBase = potionFromPanel.usedComponents.GetPotionBase();
            Managers.Potion.ApplySerializedPotionRecipeDataToCurrentPotion(potionFromPanel, potionBase, true);
            StaticStorage.PouredInUsedComponents = potion.usedComponents.GetSummaryComponents().ToList();
            StaticStorage.PouredInEffects = potion.Effects.ToList();
            if (matchingRecipe != null)
            {
                Managers.Potion.potionCraftPanel.potionChangedAfterSavingRecipe = false;
                Managers.Potion.potionCraftPanel.saveRecipeButton.Locked = true;
            }
            foreach (var collectedPotionEffect in Managers.Potion.collectedPotionEffects)
            {
                if (collectedPotionEffect == null) continue;

                foreach (var potionEffectsOn in Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap)
                {
                    if (potionEffectsOn.Effect == collectedPotionEffect)
                    {
                        potionEffectsOn.Status = PotionEffectStatus.Collected;
                        break;
                    }
                }
            }

        }
    }
}
