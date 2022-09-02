using HarmonyLib;
using PotionCraft;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Player;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.ScriptableObjects;
using PotionCraftPourBackIn.Scripts.Services;
using PotionCraftPourBackIn.Scripts.Storage;
using PotionCraftPourBackIn.Scripts.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PotionCraft.ManagersSystem.Player.PlayerManager;
using static PotionCraft.SaveLoadSystem.ProgressState;

namespace PotionCraftPourBackIn.Scripts.Patchers
{
    /// <summary>
    /// Responsible for patching potion storage and logic
    /// </summary>
    public static class PotionDataPatcher
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
        }

        public static bool OverrideAddIngredientPathToMapPath(Stack instance)
        {
            var potionStackItem = instance.itemsFromThisStack.FirstOrDefault() as PotionStackItem;
            if (potionStackItem == null) return true;
            var potion = (Potion)potionStackItem.potionItem.inventoryItem;
            ContinueBrewingFromPotion(potion);
            return false;
        }

        private static void ContinueBrewingFromPotion(Potion potion)
        {
            var matchingRecipe = RecipeService.GetRecipeForPotion(potion);
            var potionFromPanel = potion.potionFromPanel.serializedPath.indicatorTargetPosition == Vector2.zero
                                    ? matchingRecipe
                                    : potion.potionFromPanel;
            if (potionFromPanel == null)
            {
                Plugin.PluginLogger.LogInfo("ERROR: Failed to find recipe for poured-in, pre-mod potion!");
                return;
            }
            var potionBase = potionFromPanel.potionUsedComponents.Count == 0
                                ? Managers.RecipeMap.currentMap.potionBase
                                : PotionBase.GetByName(potionFromPanel.potionUsedComponents[0].componentName);
            potionFromPanel.ApplyPotionToCurrentPotion(potionBase);
            StaticStorage.PouredInUsedComponents = potion.usedComponents.ToList();
            StaticStorage.PouredInEffects = potion.Effects.ToList();
            if (matchingRecipe != null)
            {
                Managers.Potion.potionCraftPanel.potionChangedAfterSavingRecipe = false;
                Managers.Potion.potionCraftPanel.saveRecipeButton.Locked = true;
            }
            foreach (var collectedPotionEffect in Managers.Potion.collectedPotionEffects)
            {
                if (collectedPotionEffect == null) continue;

                foreach (var potionEffectsOn in Managers.RecipeMap.currentMap.potionEffectsOnMap)
                {
                    if (potionEffectsOn.effect == collectedPotionEffect)
                    {
                        potionEffectsOn.Status = PotionEffectStatus.Collected;
                        break;
                    }
                }
            }

        }

        [HarmonyPatch(typeof(Potion), "Clone")]
        public class CopyImportantInfoToPotionInstancePotionPatch
        {
            static void Postfix(Potion __result, Potion __instance)
            {
                Ex.RunSafe(() => CopyImportantInfoToPotionInstance(__result, __instance));
            }
        }

        /// <summary>
        /// This method copies important information to the potion that is normally lost unless the potion is saved as a recipe
        /// This is important so we can use the potion just like a recipe later
        /// </summary>
        public static void CopyImportantInfoToPotionInstance(Potion copyTo, Potion copyFrom)
        {
            CopyImportantInfoToPotionInstance(copyTo, copyFrom, copyFrom.potionFromPanel);
        }


        /// <summary>
        /// This method copies important information to the potion that is normally lost unless the potion is saved as a recipe
        /// This is important so we can use the potion just like a recipe later
        /// </summary>
        private static void CopyImportantInfoToPotionInstance(Potion copyTo, Potion copyFromPotion, SerializedPotionFromPanel copyFrom)
        {
            var recipeMarks = copyTo.potionFromPanel.recipeMarks;
            recipeMarks.Clear();
            copyFrom.recipeMarks.ForEach(m => recipeMarks.Add(m.Clone()));
            copyTo.potionFromPanel.collectedPotionEffects.Clear();
            foreach (var collectedPotionEffect in copyFromPotion.Effects)
            {
                if (collectedPotionEffect == null)
                    break;
                copyTo.potionFromPanel.collectedPotionEffects.Add(collectedPotionEffect.name);
            }
            copyTo.potionFromPanel.serializedPath = copyFrom.serializedPath;
            copyTo.potionFromPanel.potionUsedComponents = copyFrom.potionUsedComponents;
            copyTo.potionFromPanel.potionSkinSettings = copyFrom.potionSkinSettings;
        }

        [HarmonyPatch(typeof(PotionCraftPanel), "MakePotion")]
        public class SetCurrentlyMakingPotionPatch
        {
            static bool Prefix()
            {
                return Ex.RunSafe(() => SetCurrentlyMakingPotion(true));
            }
        }

        [HarmonyPatch(typeof(PotionCraftPanel), "MakePotion")]
        public class UnsetCurrentlyMakingPotionPatch
        {
            static void Postfix()
            {
                Ex.RunSafe(() => SetCurrentlyMakingPotion(false));
            }
        }

        public static bool SetCurrentlyMakingPotion(bool currentlyMakingPotion)
        {
            StaticStorage.CurrentlyMakingPotion = currentlyMakingPotion;
            return true;
        }



        [HarmonyPatch(typeof(SaveRecipeButton), "GenerateRecipe")]
        public class SetCurrentlyGeneratingRecipePatch
        {
            static bool Prefix()
            {
                return Ex.RunSafe(() => SetCurrentlyGeneratingRecipe(true));
            }
        }

        [HarmonyPatch(typeof(SaveRecipeButton), "GenerateRecipe")]
        public class UnsetCurrentlyGeneratingRecipePatch
        {
            static void Postfix()
            {
                Ex.RunSafe(() => SetCurrentlyGeneratingRecipe(false));
            }
        }

        public static bool SetCurrentlyGeneratingRecipe(bool currentlyGeneratingRecipe)
        {
            StaticStorage.CurrentlyGeneratingRecipe = currentlyGeneratingRecipe;
            return true;
        }



        [HarmonyPatch(typeof(PotionManager), "GeneratePotionFromCurrentPotion")]
        public class CopyImportantInfoToPotionInstancePotionManagerPatch
        {
            [HarmonyAfter(new string[] { "com.fahlgorithm.potioncraftalchemymachinerecipies" })]
            static void Postfix(Potion __result)
            {
                Ex.RunSafe(() => CopyImportantInfoToPotionInstance(__result));
            }
        }

        public static void CopyImportantInfoToPotionInstance(Potion copyTo)
        {
            if (!StaticStorage.CurrentlyMakingPotion || StaticStorage.CurrentlyGeneratingRecipe) return;

            var copyFrom = SerializedPotionFromPanel.GetPotionFromCurrentPotion();
            var copyFromPotion = Managers.Potion.potionCraftPanel.GetCurrentPotion();
            CopyImportantInfoToPotionInstance(copyTo, copyFromPotion, copyFrom);
        }


        [HarmonyPatch(typeof(StackVisualEffects), "SpawnEffectsUpdate")]
        public class HidePotionStackVisualEffectsPatch
        {
            static bool Prefix(StackVisualEffects __instance)
            {
                return Ex.RunSafe(() => !IsPotionStackItemEffect(__instance));
            }
        }

        [HarmonyPatch(typeof(StackVisualEffects))]
        public class HidePotionStackExplosionEffectsPatch
        {
            static MethodInfo TargetMethod()
            {
                return typeof(StackVisualEffects).GetMethod("SpawnEffectsExplosion", new[] { typeof(VisualEffectScriptableObject), typeof(Vector3), typeof(SpriteSortingLayers) });
            }

            static bool Prefix(StackVisualEffects __instance)
            {
                return Ex.RunSafe(() => !IsPotionStackItemEffect(__instance));
            }
        }

        public static bool IsPotionStackItemEffect(StackVisualEffects instance)
        {
            return instance?.stackScript?.inventoryItem is Potion;
        }

        [HarmonyPatch(typeof(ExperienceSubManager))]
        public class AddModifiedExperienceForPouredBackInPotionsPatch
        {
            static MethodInfo TargetMethod()
            {
                return typeof(ExperienceSubManager).GetMethod("AddExperience", new[] { typeof(Potion), typeof(int) });
            }

            static bool Prefix(Potion potion, int count)
            {
                return Ex.RunSafe(() => AddModifiedExperienceForPouredBackInPotions(potion, count));
            }
        }

        public static bool AddModifiedExperienceForPouredBackInPotions(Potion potion, int count)
        {
            if (StaticStorage.PouredInUsedComponents == null) return true;

            var asset = PotionCraft.Settings.Settings<PlayerManagerSettings>.Asset;
            var ingredientsExperience = 0.0f;
            potion.usedComponents.Skip(StaticStorage.PouredInUsedComponents.Count).ToList().ForEach(component =>
            {
                if (component.componentType != Potion.UsedComponent.ComponentType.InventoryItem) return;
                ingredientsExperience += ((InventoryItem)component.componentObject).GetPrice() * component.amount;
            });
            ingredientsExperience *= asset.experiencePotionIngredientsMultiplier;
            var desiredEffects = new List<PotionEffect>();
            var newEffectsIndex = 0;
            for (var i = 0; i < (StaticStorage.PouredInEffects?.Count ?? 0); i++)
            {
                if (potion.Effects[newEffectsIndex] == StaticStorage.PouredInEffects[i])
                {
                    newEffectsIndex++; continue;
                }
                if (newEffectsIndex > 0) break;
            }
            var newEffects = potion.Effects.Skip(newEffectsIndex).ToList();
            var num = newEffects.Any() 
                        ? potion.GetPotionReview(newEffects.ToArray()).cost * asset.experiencePotionEffectsMultiplier 
                        : 0f;
            Managers.Player.AddExperience((ingredientsExperience + num) * count);

            StaticStorage.PouredInUsedComponents = null;
            StaticStorage.PouredInEffects = null;
            return false;
        }

    }
}
