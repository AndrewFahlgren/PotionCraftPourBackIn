using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ScriptableObjects;
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
            return potion.potionFromPanel.serializedPath.indicatorTargetPosition != Vector2.zero || (potion.potionFromPanel?.serializedPath?.fixedPathPoints?.Any() ?? false);
        }

        public static bool IsPotionStackItemEffect(StackVisualEffects instance)
        {
            return instance?.stackScript?.inventoryItem is Potion;
        }


        /// <summary>
        /// This method copies important information to the potion that is normally lost unless the potion is saved as a recipe
        /// This is important so we can use the potion just like a recipe later
        /// </summary>
        public static void CopyImportantInfoToPotionInstance(Potion copyTo, Potion copyFromPotion, SerializedPotionFromPanel copyFrom)
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
    }
}
