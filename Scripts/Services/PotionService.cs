using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using PotionCraft.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PotionCraftPourBackIn.Scripts.Services
{
    /// <summary>
    /// Responsible for managing potion storage and logic
    /// </summary>
    public static class PotionService
    {

        public static void ContinueBrewingFromPotion(Potion potion)
        {
            var potionFromPanel = potion.potionFromPanel;
            var potionBase = potionFromPanel.potionUsedComponents.Count == 0 
                                ? Managers.RecipeMap.currentMap.potionBase 
                                : PotionBase.GetByName(potionFromPanel.potionUsedComponents[0].componentName);
            potionFromPanel.ApplyPotionToCurrentPotion(potionBase); //TODO make sure we don't remove ingredients when we do this
            //TODO make sure we update enablement on recipe buttons when we do this
            foreach (var collectedPotionEffect in Managers.Potion.collectedPotionEffects)
            {
                if (!(collectedPotionEffect == null))
                {
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
        }

        public static void CopyImportantInfoToPotionInstance(Potion copyTo, PotionManager instance)
        {
            var copyFrom = instance.potionCraftPanel.GetCurrentPotion();
            CopyImportantInfoToPotionInstance(copyTo, copyFrom);
        }

        /// <summary>
        /// This method copies important information to the potion that is normally lost unless the potion is saved as a recipe
        /// This is important so we can use the potion just like a recipe later
        /// </summary>
        public static void CopyImportantInfoToPotionInstance(Potion copyTo, Potion copyFrom)
        {
            var recipeMarks = copyTo.potionFromPanel.recipeMarks;
            copyFrom.potionFromPanel.recipeMarks.ForEach(m => recipeMarks.Add(m.Clone()));
            copyTo.potionFromPanel.collectedPotionEffects = copyFrom.potionFromPanel.collectedPotionEffects;

            //TODO also copy other things from the SerializedPotionFromPanel like the current path and who knows what else
        }
    }
}
