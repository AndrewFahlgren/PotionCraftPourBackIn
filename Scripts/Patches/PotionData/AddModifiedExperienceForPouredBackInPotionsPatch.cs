using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Player;
using PotionCraft.ScriptableObjects;
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
            return typeof(ExperienceSubManager).GetMethod("AddExperience", new[] { typeof(Potion), typeof(int) });
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
    }
}
