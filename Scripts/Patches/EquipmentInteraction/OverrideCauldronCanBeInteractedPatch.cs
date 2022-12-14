using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Input.ControlProviders;
using PotionCraft.ObjectBased.Cauldron;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.Potion;
using HarmonyLib;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraftPourBackIn.Scripts.Services;
using UnityEngine;
using PotionCraft.Assemblies.GamepadNavigation;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    public class OverrideCauldronCanBeInteractedPatch
    {
        [HarmonyPatch(typeof(Cauldron), "CanBeInteractedNow")]
        public class Cauldron_CanBeInteractedNow
        {
            static void Postfix(ref bool __result, Cauldron __instance)
            {
                OverrideCauldronCanBeInteracted(ref __result, __instance);
            }
        }

        [HarmonyPatch(typeof(SlotSelector), "SelectNextSlotByStick")]
        public class SlotSelector_SelectNextSlotByStick
        {
            static bool Prefix()
            {
                SetIsSelectingSlotByStick(true);
                return true;
            }
            static void Postfix()
            {
                SetIsSelectingSlotByStick(false);
            }
        }


        private static void OverrideCauldronCanBeInteracted(ref bool result, Cauldron instance)
        {
            if (result == true) return;
            var newResult = false;
            Ex.RunSafe(() =>
            {
                var grabbedInteractiveItem = Managers.Cursor.grabbedInteractiveItem;
                if (grabbedInteractiveItem is not PotionItem potionItem) return;
                //Do not allow potion interactions if we are already brewing
                if (Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return;
                //Do not show highlight and allow interactions for potions which cannot be poured back in
                if (!PotionDataService.PotionHasSerializedData((Potion)potionItem.inventoryItem)
                    && RecipeService.GetRecipeForPotion((Potion)potionItem.inventoryItem) == null) return;
                //We need to dissallow vacuuming outside of certain bounds to prevent animation issues.
                if (!IsSelectingSlotByStick && !IsPotionItemWithinCauldronBounds(instance, potionItem)) return;
                //Now rerun some of the logic within Cauldron to determine if we should allow interaction
                var controlsProvider = Managers.Input.controlsProvider;
                newResult = instance.СanTakeIngredients
                            && (potionItem.GetRigidbody() == null
                                    || InteractiveItem.CanBeInteractedByVelocity(potionItem.GetRigidbody().velocity)
                                    || controlsProvider is Gamepad
                                    && !Gamepad.MustFreeModeBeEnabled());
            });
            if (newResult) result = true;
        }

        private static bool IsPotionItemWithinCauldronBounds(Cauldron instance, PotionItem potionItem)
        {
            var waterbounds = instance.waterZoneCollider.bounds;
            //For whatever reason the vacuum animation doesn't work when the potion item is outside of these bounds
            if (potionItem.transform.position.x <= waterbounds.min.x || potionItem.transform.position.x >= waterbounds.max.x) return false;
            var physicsCollider = instance.GetComponentInChildren<CauldronPhysicsCollider>()?.GetComponent<PolygonCollider2D>();
            if (physicsCollider != null)
            {
                //We also seem to have issues with the vacuum animation when we are lower down in the cauldron
                //determine how low we are in the cauldron
                var lownessPercentile = (potionItem.transform.position.y - physicsCollider.bounds.min.y) / physicsCollider.bounds.size.y;
                //If we are below halfway that is too far
                if (lownessPercentile < 0.5f) return false;
                //We also need to reduce how far to the sides we go as we go lower. The bug seems to happen in a rough radius around the water bounds.
                if (lownessPercentile >= 0.7f) lownessPercentile = 1;
                else if (lownessPercentile > 0.5f) lownessPercentile += 0.2f;
                var xReduction = 0.7f * (1 - lownessPercentile) * waterbounds.size.x;
                if (potionItem.transform.position.x <= waterbounds.min.x + xReduction || potionItem.transform.position.x >= waterbounds.max.x - xReduction) return false;
            }
            return true;
        }

        private static bool IsSelectingSlotByStick;
        //We need to relax interaction rules when using a controller so that the cauldron slot can be selected even when the potion is far away from the cauldron
        private static void SetIsSelectingSlotByStick(bool isSelectingSlotByStick)
        {
            IsSelectingSlotByStick = isSelectingSlotByStick;
        }
    }
}
