using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Player;
using PotionCraft.ManagersSystem.Potion.Entities;
using PotionCraft.ScriptableObjects;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraftPourBackIn.Scripts.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static PotionCraft.ManagersSystem.Player.PlayerManager;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(ExperienceSubManager))]
    public class AddModifiedExperienceForPouredBackInPotionsPatch
    {
        static MethodInfo TargetMethod()
        {
            return typeof(ExperienceSubManager).GetMethod("AddExperience", [typeof(Potion), typeof(int)]);
        }

        static bool Prefix(Potion potion, int count)
        {
            return Ex.RunSafe(() => AddModifiedExperienceForPouredBackInPotions(potion, count));
        }

        private static bool AddModifiedExperienceForPouredBackInPotions(Potion potion, int count)
        {
            if (StaticStorage.PouredInUsedComponents == null) return true;

            var asset = PotionCraft.Settings.Settings<PlayerManagerSettings>.Asset;
            var ingredientsExperience = 0.0f;
            var usedComponents = potion.usedComponents.GetSummaryComponents().Any() ? potion.usedComponents : Managers.Potion.PotionUsedComponents;
            usedComponents.GetSummaryComponents().Skip(StaticStorage.PouredInUsedComponents.Count).ToList().ForEach(component =>
            {
                if (component.Type != AlchemySubstanceComponentType.InventoryItem) return;
                ingredientsExperience += ((InventoryItem)component.Component).GetPrice() * component.Amount;
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
                        ? Potion.GetPotionReview([], [.. newEffects]).cost * asset.experiencePotionEffectsMultiplier
                        : 0f;
            Managers.Player.AddExperience((ingredientsExperience + num) * count);

            StaticStorage.PouredInUsedComponents = null;
            StaticStorage.PouredInEffects = null;
            return false;
        }
    }
}
