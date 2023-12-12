using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.ScriptableObjects;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraftPourBackIn.Scripts.Storage;
using System.Linq;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    public static partial class UnfinishedPotionPatcher
    {
        [HarmonyPatch(typeof(PotionInventoryObject), "CanBeInteractedNow")]
        public class EnableGrabUnfinishedPotionToFinishPatch
        {
            static bool Prefix(InventoryItem ___inventoryItem)
            {
                return Ex.RunSafe(() => EnableGrabUnfinishedPotionToFinishPrefix(___inventoryItem));
            }
            static void Postfix(InventoryItem ___inventoryItem)
            {
                Ex.RunSafe(() => EnableGrabUnfinishedPotionToFinishPostfix(___inventoryItem));
            }
        }

        [HarmonyPatch(typeof(RecipeBookPotionInventoryObject), "CanBeInteractedNow")]
        public class RecipeBookPotionInventoryObject_CanBeInteractedNow
        {
            static bool Prefix(InventoryItem ___inventoryItem)
            {
                return Ex.RunSafe(() => EnableGrabUnfinishedPotionToFinishPrefix(___inventoryItem, false));
            }
            static void Postfix(InventoryItem ___inventoryItem)
            {
                Ex.RunSafe(() => EnableGrabUnfinishedPotionToFinishPostfix(___inventoryItem, false));
            }
        }

        private static bool EnableGrabUnfinishedPotionToFinishPrefix(InventoryItem inventoryItem, bool requireBrewStart = true)
        {
            if (inventoryItem is not Potion potion) return true;
            if (requireBrewStart && !Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return true;
            if (potion.Effects.Any()) return true;
            potion.Effects = new[] { PotionEffect.allPotionEffects.First() };
            StaticStorage.DummyEffectAddedToEnableFinishPotionGrab = true;
            return true;
        }

        private static void EnableGrabUnfinishedPotionToFinishPostfix(InventoryItem inventoryItem, bool requireBrewStart = true)
        {
            if (inventoryItem is not Potion potion) return;
            if (requireBrewStart && !Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return;
            if (!StaticStorage.DummyEffectAddedToEnableFinishPotionGrab) return;
            potion.Effects = new PotionEffect[0];
            StaticStorage.DummyEffectAddedToEnableFinishPotionGrab = false;
        }
    }
}
