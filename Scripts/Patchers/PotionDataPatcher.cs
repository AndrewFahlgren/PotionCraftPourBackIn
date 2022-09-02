using HarmonyLib;
using PotionCraft;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Player;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.SaveLoadSystem;
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
            var potionFromPanel = !PotionHasSerializedData(potion)
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

        public static bool PotionHasSerializedData(Potion potion)
        {
            return potion.potionFromPanel.serializedPath.indicatorTargetPosition != Vector2.zero || (potion.potionFromPanel?.serializedPath?.fixedPathPoints?.Any() ?? false);
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
            foreach (var collectedPotionEffect in copyFromPotion?.Effects ?? Managers.Potion.collectedPotionEffects)
            {
                if (collectedPotionEffect == null)
                    break;
                copyTo.potionFromPanel.collectedPotionEffects.Add(collectedPotionEffect.name);
            }
            copyTo.potionFromPanel.serializedPath = copyFrom.serializedPath;
            if (!copyTo.usedComponents?.Any() ?? false)
            {
                if (copyTo.usedComponents == null) copyTo.usedComponents = new List<Potion.UsedComponent>();
                copyTo.usedComponents = Managers.Potion.usedComponents.Select(component => component.Clone()).ToList();
            }
            if (!copyFrom.potionUsedComponents.Any())
            {
                copyTo.usedComponents.ForEach((component) =>
                {
                    copyFrom.potionUsedComponents.Add(new SerializedUsedComponent
                    {
                        componentName = component.componentObject.name,
                        componentAmount = component.amount,
                        componentType = component.componentType.ToString()
                    });
                });
            }
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
            PotionItem linkedPotionItem = null;
            if (copyFromPotion == null)
            {
                if (Managers.Cursor.grabbedInteractiveItem is PotionItem potionItem)
                {
                    potionItem.inventoryItem = copyTo;
                    linkedPotionItem = potionItem;
                }
            }
            CopyImportantInfoToPotionInstance(copyTo, copyFromPotion, copyFrom);
            if (linkedPotionItem != null) PotionItemStackPatcher.SetupPotionItemForPouringIn(linkedPotionItem);
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
            var usedComponents = potion.usedComponents.Any() ? potion.usedComponents : Managers.Potion.usedComponents;
            usedComponents.Skip(StaticStorage.PouredInUsedComponents.Count).ToList().ForEach(component =>
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


        [HarmonyPatch(typeof(PotionCraftPanel), "UpdatePotionInCraftPanel")]
        public class KeepTrackOfCurrentPotionCraftPanelPotionPatch
        {
            static void Postfix()
            {
                Ex.RunSafe(() => KeepTrackOfCurrentPotionCraftPanelPotion());
            }
        }

        public static void KeepTrackOfCurrentPotionCraftPanelPotion()
        {
            StaticStorage.CurrentPotionCraftPanelPotion = Managers.Potion.potionCraftPanel.GetCurrentPotion();
        }


        [HarmonyPatch(typeof(Potion), "GetSerializedInventorySlot")]
        public class SavePotionSerializedDataPatch
        {
            static void Postfix(SerializedInventorySlot __result, Potion __instance)
            {
                Ex.RunSafe(() => SavePotionSerializedData(__result, __instance));
            }
        }

        public static void SavePotionSerializedData(SerializedInventorySlot result, Potion instance)
        {
            var dataToInsert = $",\"potionFromPanel\":{JsonUtility.ToJson(instance.potionFromPanel)}";
            var insertIndex = result.data.LastIndexOf('}');
            result.data = result.data.Insert(insertIndex, dataToInsert);
        }

        [HarmonyPatch(typeof(Potion))]
        public class LoadPotionSerializedDataPatch
        {
            static MethodInfo TargetMethod()
            {
                return typeof(Potion).GetMethod("GetFromSerializedObject", new[] { typeof(SerializedInventorySlot) });
            }

            static void Postfix(Potion __result, SerializedInventorySlot serializedObject)
            {
                Ex.RunSafe(() => LoadPotionSerializedData(__result, serializedObject));
            }
        }

        public static void LoadPotionSerializedData(Potion result, SerializedInventorySlot serializedRecipe)
        {
            //Check if there is an existing potionFromPanel
            var keyIndex = serializedRecipe.data.IndexOf("potionFromPanel");
            if (keyIndex == -1)
            {
                return;
            }
            //Determine the start of the object
            var startPotionFromPanelIndex = serializedRecipe.data.IndexOf('{', keyIndex);
            if (startPotionFromPanelIndex == -1)
            {
                Plugin.PluginLogger.LogInfo("Error: potionFromPanel data in serialized potion is malformed.");
                return;
            }
            //Find the closing bracket of the list
            var endPotionFromPanelIndex = GetEndJsonIndex(serializedRecipe.data, startPotionFromPanelIndex, false);
            if (endPotionFromPanelIndex >= serializedRecipe.data.Length)
            {
                Plugin.PluginLogger.LogInfo("Error: potionFromPanel data in serialized potion is malformed (bad end index).");
                return;
            }

            var savedPotionFromPanelJson = serializedRecipe.data.Substring(startPotionFromPanelIndex, endPotionFromPanelIndex - startPotionFromPanelIndex);
            if (savedPotionFromPanelJson.Length <= 2)
            {
                Plugin.PluginLogger.LogInfo("Error: potionFromPanel data in serialized potion is malformed (empty object).");
                return;
            }

            result.potionFromPanel = JsonUtility.FromJson<SerializedPotionFromPanel>(savedPotionFromPanelJson);
        }

        /// <summary>
        /// Manually parses the json to find the closing bracket for this json object.
        /// </summary>
        /// <param name="input">the json string to parse.</param>
        /// <param name="startIndex">the openning bracket of this object/list.</param>
        /// <param name="useBrackets">if true this code will look for closing brackets and if false this code will look for curly braces.</param>
        /// <returns></returns>
        private static int GetEndJsonIndex(string input, int startIndex, bool useBrackets)
        {
            var endIndex = startIndex + 1;
            var unclosedCount = 1;
            var openChar = useBrackets ? '[' : '{';
            var closeChar = useBrackets ? ']' : '}';
            while (unclosedCount > 0 && endIndex < input.Length - 1)
            {
                var nextOpenIndex = input.IndexOf(openChar, endIndex);
                var nextCloseIndex = input.IndexOf(closeChar, endIndex);
                if (nextOpenIndex == -1 && nextCloseIndex == -1)
                {
                    break;
                }
                if (nextOpenIndex == -1) nextOpenIndex = int.MaxValue;
                if (nextCloseIndex == -1) nextCloseIndex = int.MaxValue;
                if (nextOpenIndex < nextCloseIndex)
                {
                    endIndex = nextOpenIndex + 1;
                    unclosedCount++;
                }
                else if (nextCloseIndex < nextOpenIndex)
                {
                    endIndex = nextCloseIndex + 1;
                    unclosedCount--;
                }
            }
            return endIndex;
        }
    }
}
