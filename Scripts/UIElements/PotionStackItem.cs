using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack.StackItem;
using PotionCraftPourBackIn.Scripts.Services;
using UnityEngine;

namespace PotionCraftPourBackIn.Scripts.UIElements
{
    /// <summary>
    /// Dummy script for marking stack items as potion stack items
    /// </summary>
    public class PotionStackItem : StackItem
    {
        public PotionItem potionItem;

        public override Bounds? GetHoveringColliderBounds()
        {
            return potionItem.mainCollider.bounds;
        }

       
        public override void SetPhysicsToDefault()
        {
            potionItem.gameObject.layer = 0;
        }

        public override void SetThisItemLayersToNormal()
        {
            potionItem.gameObject.layer = 14;
        }

        public override void IgnoreCollision(Collider2D target, bool ignore = true)
        {
            if (target.GetComponentInParent<ThrowVacuuming>() != Managers.Ingredient.cauldron.throwVacuuming) return;
            if (!ignore && Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return;
            potionItem.IgnoreCollision(target, ignore);
        }
    }
}
