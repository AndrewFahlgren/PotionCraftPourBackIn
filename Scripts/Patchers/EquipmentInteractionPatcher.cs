using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Input.ControlProviders;
using PotionCraft.ObjectBased.Cauldron;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.Stack.StackItem;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using PotionCraft;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.UIElements;

namespace PotionCraftPourBackIn.Scripts.Patchers
{
    /// <summary>
    /// Responsible for patching interactions with the cauldron and mortar
    /// </summary>
    public static class EquipmentInteractionPatcher
    {
        #region Interaction Overrides

        [HarmonyPatch(typeof(PotionItem), "VacuumItem")]
        public class OverridePotionItemVaccumItemPatch
        {
            static bool Prefix(PotionItem __instance, bool isPrimaryGrab, bool forceMassModifier, bool forceAltModifier)
            {
                return Ex.RunSafe(() => OverridePotionItemVaccumItem(__instance, isPrimaryGrab, forceMassModifier, forceAltModifier));
            }
        }

        public static bool OverridePotionItemVaccumItem(PotionItem instance, bool isPrimaryGrab, bool forceMassModifier, bool forceAltModifier)
        {
            if (!isPrimaryGrab) return true;

            var stack = instance.gameObject.GetComponent<Stack>(); ;
            stack.VacuumItem(isPrimaryGrab, forceMassModifier, forceAltModifier);

            return false;
        }

        [HarmonyPatch(typeof(PotionItem), "CanBeInteractedNow")]
        public class PreventPotionInteractionWhileVacuumPatch
        {
            static void Postfix(ref bool __result, PotionItem __instance)
            {
                PreventPotionInteractionWhileVacuum(ref __result, __instance);
            }
        }

        public static void PreventPotionInteractionWhileVacuum(ref bool result, PotionItem instance)
        {
            if (result == false) return;
            var newResult = true;
            Ex.RunSafe(() =>
            {
                var stack = instance.GetComponent<Stack>();
                if (stack == null) return;
                newResult = stack.vacuumingTo == null && instance.GetComponent<SortingGroup>().sortingLayerID != (int)SpriteSortingLayers.IngredientsInCauldron;
            });
            if (!newResult) result = false;
        }

        [HarmonyPatch(typeof(GraphicStateMachine), "SetCurrentGraphicState")]
        public class RemovePotionHighlightWhenEnteringCauldronPatch
        {
            static void Postfix(GraphicStateMachine __instance)
            {
                Ex.RunSafe(() => RemovePotionHighlightWhenEnteringCauldron(__instance));
            }
        }

        public static void RemovePotionHighlightWhenEnteringCauldron(GraphicStateMachine instance)
        {
            var stackItem = instance.stackItem;
            if (stackItem.sortingGroup.sortingLayerName == SpriteSortingLayers.IngredientsInCauldron.ToString())
            {
                var stackHighlight = stackItem.stackScript.iHoverableScript as PotionItem;
                if (stackHighlight != null)
                {
                    stackHighlight.SetHovered(hover: false);
                }
            }
        }

        [HarmonyPatch(typeof(Cauldron), "CanBeInteractedNow")]
        public class OverrideCauldronCanBeInteractedPatch
        {
            static void Postfix(ref bool __result, Cauldron __instance)
            {
                OverrideCauldronCanBeInteracted(ref __result, __instance);
            }
        }

        public static void OverrideCauldronCanBeInteracted(ref bool result, Cauldron instance)
        {
            if (result == true) return;
            var newResult = false;
            Ex.RunSafe(() =>
            {
                var grabbedInteractiveItem = Managers.Cursor.grabbedInteractiveItem;
                if (grabbedInteractiveItem is not PotionItem potionItem) return;
                if (Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return;
                var controlsProvider = Managers.Input.controlsProvider;
                newResult = instance.СanTakeIngredients
                            && (potionItem.GetRigidbody() == null
                                 || InteractiveItem.CanBeInteractedByVelocity(potionItem.GetRigidbody().velocity)
                                 || controlsProvider is Gamepad
                                 && !Gamepad.MustFreeModeBeEnabled());
            });
            if (newResult) result = true;
        }

        #endregion

        #region Collider Overrides

        [HarmonyPatch(typeof(WaterZone), "OnTriggerStay2D")]
        public class RejectPotionCollisionWhenBrewingPatch
        {
            static bool Prefix(Collider2D other)
            {
                return Ex.RunSafe(() => RejectPotionCollisionWhenBrewing(other));
            }
        }

        public static bool RejectPotionCollisionWhenBrewing(Collider2D other)
        {
            var potionItem = other.GetComponentInParent<PotionItem>();
            if (potionItem == null) return true;

            if (Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return false;

            return true;
        }

        [HarmonyPatch(typeof(EnterZone), "OnTriggerStay2D")]
        public class AcceptPotionCollisionEnterZonePatch
        {
            static bool Prefix(Collider2D other, ThrowVacuuming ___throwVacuuming)
            {
                return Ex.RunSafe(() => DisallowMortarPotionCollisions(other, ___throwVacuuming));
            }
        }

        [HarmonyPatch(typeof(EnterZone), "OnTriggerExit2D")]
        public class OverrideEnterZoneOnTriggerExit2DPatch
        {
            static bool Prefix(Collider2D other, ThrowVacuuming ___throwVacuuming)
            {
                return Ex.RunSafe(() => DisallowMortarPotionCollisions(other, ___throwVacuuming));
            }
        }

        public static bool DisallowMortarPotionCollisions(Collider2D other, ThrowVacuuming throwVacuuming)
        {
            var stackItem = other.GetComponentInChildren<PotionStackItem>();
            if (stackItem == null) return true;
            if (throwVacuuming == Managers.Ingredient.cauldron.throwVacuuming) return true;
            return false;
        }

        [HarmonyPatch(typeof(GraphicStateMachine), "OnTriggerEnter2D")]
        public class DisallowMortarGraphicStateSwitchPatch
        {
            static bool Prefix(GraphicStateMachine __instance, Collider2D other)
            {
                return Ex.RunSafe(() => DisallowMortarGraphicStateSwitch(__instance, other));
            }
        }

        [HarmonyPatch(typeof(GraphicStateMachine), "OnTriggerExit2D")]
        public class DisallowMortarGraphicStateSwitchExitPatch
        {
            static bool Prefix(GraphicStateMachine __instance, Collider2D other)
            {
                return Ex.RunSafe(() => DisallowMortarGraphicStateSwitch(__instance, other));
            }
        }

        public static bool DisallowMortarGraphicStateSwitch(GraphicStateMachine instance, Collider2D other)
        {
            if (instance.stackItem is not PotionStackItem) return true;
            if (Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return false;
            return other.GetComponentInParent<InteractiveItem>() is not PotionCraft.ObjectBased.Mortar.Mortar;
        }

        [HarmonyPatch(typeof(Stack), "OnCollisionEnter2D")]
        public class OverrideStackOnCollisionEnter2DPatch
        {
            static bool Prefix(Stack __instance)
            {
                return Ex.RunSafe(() => OverrideStackOnCollisionEnter2D(__instance));
            }
        }

        public static bool OverrideStackOnCollisionEnter2D(Stack instance)
        {
            if (instance.itemsFromThisStack.FirstOrDefault() is PotionStackItem) return false;
            return true;
        }

        #endregion

        #region Helper Methods

        public static bool IsColliderCauldronThrowVacuumingPhysics(Collider2D target)
        {
            return target.GetComponentInParent<ThrowVacuuming>() == Managers.Ingredient.cauldron.throwVacuuming;
        }

        public static void IgnoreCollisionForPotionItem(PotionItem potionItem, Collider2D target, bool ignore)
        {
            if (!ignore && Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return;
            potionItem.IgnoreCollision(target, ignore);
        }

        #endregion
    }
}
