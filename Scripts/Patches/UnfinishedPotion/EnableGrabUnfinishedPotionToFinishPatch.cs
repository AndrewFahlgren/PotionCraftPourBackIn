using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.ScriptableObjects;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraftPourBackIn.Scripts.Storage;
using PotionCraftPourBackIn.Scripts.Extensions;
using System.Linq;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    public static partial class UnfinishedPotionPatcher
    {
        [HarmonyPatch(typeof(PotionInventoryObject), "CanBeInteractedNow")]
        public class EnableGrabUnfinishedPotionToFinishPatch
        {
            static bool Prefix(PotionInventoryObject __instance)
            {
                return Ex.RunSafe(() => EnableGrabUnfinishedPotionToFinishPrefix(__instance.GetInventoryItem()));
            }
            static void Postfix(PotionInventoryObject __instance)
            {
                Ex.RunSafe(() => EnableGrabUnfinishedPotionToFinishPostfix(__instance.GetInventoryItem()));
            }
        }

        [HarmonyPatch(typeof(RecipeBookInventoryObjectPotion), "CanBeInteractedNow")]
        public class RecipeBookPotionInventoryObject_CanBeInteractedNow
        {
            static bool Prefix(RecipeBookInventoryObjectPotion __instance)
            {
                return Ex.RunSafe(() => EnableGrabUnfinishedPotionToFinishPrefix(__instance.GetInventoryItem(), false));
            }
            static void Postfix(RecipeBookInventoryObjectPotion __instance)
            {
                Ex.RunSafe(() => EnableGrabUnfinishedPotionToFinishPostfix(__instance.GetInventoryItem(), false));
            }
        }

        private static bool EnableGrabUnfinishedPotionToFinishPrefix(InventoryItem inventoryItem, bool requireBrewStart = true)
        {
            if (inventoryItem is not Potion potion) return true;
            if (requireBrewStart && !Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return true;
            if (potion.Effects.Any()) return true;
            potion.Effects = [PotionEffect.allPotionEffects.First()];
            StaticStorage.DummyEffectAddedToEnableFinishPotionGrab = true;
            return true;
        }

        private static void EnableGrabUnfinishedPotionToFinishPostfix(InventoryItem inventoryItem, bool requireBrewStart = true)
        {
            if (inventoryItem is not Potion potion) return;
            if (requireBrewStart && !Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return;
            if (!StaticStorage.DummyEffectAddedToEnableFinishPotionGrab) return;
            potion.Effects = [];
            StaticStorage.DummyEffectAddedToEnableFinishPotionGrab = false;
        }
    }
}
