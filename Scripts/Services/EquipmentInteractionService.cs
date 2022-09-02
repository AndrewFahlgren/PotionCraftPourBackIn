using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack.StackItem;
using UnityEngine;
using PotionCraftPourBackIn.Scripts.UIElements;

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
            if (throwVacuuming == Managers.Ingredient.cauldron.throwVacuuming) return true;
            return false;
        }

        public static bool DisallowMortarGraphicStateSwitch(GraphicStateMachine instance, Collider2D other)
        {
            if (instance.stackItem is not PotionStackItem) return true;
            if (Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return false;
            return other.GetComponentInParent<InteractiveItem>() is not PotionCraft.ObjectBased.Mortar.Mortar;
        }

        public static bool IsColliderCauldronThrowVacuumingPhysics(Collider2D target)
        {
            return target.GetComponentInParent<ThrowVacuuming>() == Managers.Ingredient.cauldron.throwVacuuming;
        }

        public static void IgnoreCollisionForPotionItem(PotionItem potionItem, Collider2D target, bool ignore)
        {
            if (!ignore && Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return;
            potionItem.IgnoreCollision(target, ignore);
        }
    }
}
