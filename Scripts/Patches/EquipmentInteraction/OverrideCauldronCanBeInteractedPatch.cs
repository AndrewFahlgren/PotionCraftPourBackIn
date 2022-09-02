using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Input.ControlProviders;
using PotionCraft.ObjectBased.Cauldron;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.Potion;
using HarmonyLib;
using PotionCraft.ScriptableObjects;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(Cauldron), "CanBeInteractedNow")]
    public class OverrideCauldronCanBeInteractedPatch
    {
        static void Postfix(ref bool __result, Cauldron __instance)
        {
            OverrideCauldronCanBeInteracted(ref __result, __instance);
        }
        private static void OverrideCauldronCanBeInteracted(ref bool result, Cauldron instance)
        {
            if (result == true) return;
            var newResult = false;
            Ex.RunSafe(() =>
            {
                var grabbedInteractiveItem = Managers.Cursor.grabbedInteractiveItem;
                if (grabbedInteractiveItem is not PotionItem potionItem) return;
                if (Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return;
                //Do not show highlight and allow interactions for potions which cannot be poured back in
                if (!PotionDataService.PotionHasSerializedData((Potion)potionItem.inventoryItem)
                    && RecipeService.GetRecipeForPotion((Potion)potionItem.inventoryItem) == null) return;
                var controlsProvider = Managers.Input.controlsProvider;
                newResult = instance.СanTakeIngredients
                            && (potionItem.GetRigidbody() == null
                                    || InteractiveItem.CanBeInteractedByVelocity(potionItem.GetRigidbody().velocity)
                                    || controlsProvider is Gamepad
                                    && !Gamepad.MustFreeModeBeEnabled());
            });
            if (newResult) result = true;
        }
    }
}
