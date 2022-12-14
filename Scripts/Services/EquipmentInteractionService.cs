using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack.StackItem;
using UnityEngine;
using PotionCraftPourBackIn.Scripts.UIElements;
using PotionCraft.Assemblies.GamepadNavigation;
using System;
using PotionCraft.ObjectBased.Cauldron;
using System.Linq;
using PotionCraft.Assemblies.GamepadNavigation.Conditions;

namespace PotionCraftPourBackIn.Scripts.Services
{
    /// <summary>
    /// Responsible for patching interactions with the cauldron and mortar
    /// </summary>
    public static class EquipmentInteractionService
    {
        public static bool DisallowMortarPotionCollisions(Collider2D other, ThrowVacuuming throwVacuuming)
        {
            var stackItem = other.GetComponentInChildren<PotionStackItem>();
            if (stackItem == null) return true;
            if (stackItem.IsVacuumingNow) return false;
            if (throwVacuuming == Managers.Ingredient.cauldron.throwVacuuming) return true;
            return false;
        }

        public static bool DisallowMortarGraphicStateSwitch(GraphicStateMachine instance, Collider2D other)
        {
            if (instance.stackItem is not PotionStackItem) return true;
            if (Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return false;
            return other.GetComponentInParent<InteractiveItem>() is not PotionCraft.ObjectBased.Mortar.Mortar;
        }

        public static bool SetupSlotCauldronSlotConditionToAllowPotion(Slot instance)
        {
            var cauldron = instance.GetComponentInParent<Cauldron>();
            if (cauldron == null) return true;
            //Cauldron has many child slots. Make sure we only change the one meant for ingredients.
            if (instance.transform.parent.gameObject.name != "Cauldron") return true;
            //Make sure we don't double add conditions if this somehow gets called more than once
            if (instance.conditions.Contains(Condition.PotionInHand)) return true;
            //Make sure we are changing the correct slot by checking for this condition
            if (!instance.conditions.Contains(Condition.IngredientInHand)) return true;
            instance.conditions = instance.conditions.Concat(new[] { Condition.PotionInHand }).ToArray();
            return true;
        }
    }
}
