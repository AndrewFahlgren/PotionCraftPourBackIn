using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion.Entities;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ScriptableObjects;
using PotionCraft.ScriptableObjects.Potion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PotionCraft.SaveLoadSystem.ProgressState;

namespace PotionCraftPourBackIn.Scripts.Services
{
    /// <summary>
    /// Responsible for patching potion storage and logic
    /// </summary>
    public static class PotionDataService
    {
        public static bool PotionHasSerializedData(Potion potion)
        {
            var serializedPath = ((SerializedPotionRecipeData)potion.GetSerializedRecipeData()).serializedPath;
            return serializedPath.indicatorTargetPosition != Vector2.zero 
                   || (serializedPath?.fixedPathPoints?.Any() ?? false);
        }

        public static bool IsPotionStackItemEffect(StackVisualEffects instance)
        {
            if (instance?.stackScript == null) return false;
            return Traverse.Create(instance.stackScript).Property<InventoryItem>("InventoryItem").Value is Potion;
        }


        /// <summary>
        /// This method copies important information to the potion that is normally lost unless the potion is saved as a recipe
        /// This is important so we can use the potion just like a recipe later
        /// </summary>
        public static void CopyImportantInfoToPotionInstance(Potion copyTo, Potion copyFromPotion, SerializedPotionRecipeData copyFrom)
        {
            var copyToPotionFromPanel = (SerializedPotionRecipeData)copyTo.GetSerializedRecipeData();
            var recipeMarks = copyToPotionFromPanel.recipeMarks;
            recipeMarks.Clear();
            copyFrom.recipeMarks.ForEach(m => recipeMarks.Add(m.Clone()));
            copyToPotionFromPanel.collectedPotionEffects.Clear();
            foreach (var collectedPotionEffect in copyFromPotion?.Effects ?? Managers.Potion.collectedPotionEffects)
            {
                if (collectedPotionEffect == null)
                    break;
                copyToPotionFromPanel.collectedPotionEffects.Add(collectedPotionEffect.name);
            }
            copyToPotionFromPanel.serializedPath = copyFrom.serializedPath;
            if (!copyTo.usedComponents?.GetSummaryComponents()?.Any() ?? false)
            {
                copyTo.usedComponents.Clear();
                var mapComponents = Managers.Potion.PotionUsedComponents.GetSummaryComponents().Select(component => component.Clone()).ToList();
                mapComponents.ForEach(c => copyTo.usedComponents.Add(c));
            }
            if (!copyFrom.usedComponents.components.Any())
            {
                copyTo.usedComponents.GetSummaryComponents().ForEach((component) =>
                {
                    copyFrom.usedComponents.components.Add(new SerializedAlchemySubstanceComponent
                    {
                        name = component.Component.name,
                        amount = component.Amount,
                        type = component.Type.ToString()
                    });
                });
            }
            copyToPotionFromPanel.usedComponents = copyFrom.usedComponents;
            copyToPotionFromPanel.skinSettings = copyFrom.skinSettings;
        }
    }
}
