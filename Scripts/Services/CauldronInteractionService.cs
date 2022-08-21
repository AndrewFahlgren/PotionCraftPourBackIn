using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PotionCraftPourBackIn.Scripts.Services
{
    /// <summary>
    /// Responsible for cauldron interactions like drop detection and animations
    /// </summary>
    public static class CauldronInteractionService
    {
        public static bool AcceptPotionCollision(Collider2D other)
        {
            var potionItem = other.GetComponentInParent<PotionItem>();
            if (potionItem == null) return true;

            if (Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return false;

            var potion = potionItem.inventoryItem as Potion;

            PotionService.ContinueBrewingFromPotion(potion);
            AnimateSplash(potion);
            DeletePotion(potion);

            return false;
        }

        public static void AnimateSplash(Potion potion)
        {
            //check out how WaterZone.OnTriggerStay2D does it
        }

        public static void DeletePotion(Potion potion)
        {
            //here we need to remove the potion from the game world
        }



        public static bool AllowCauldronCollision(Collider2D target, bool ignore)
        {
            if (!ignore) return true;
            var cauldronVaccum = Managers.Ingredient.cauldron.throwVacuuming;
            var cauldronCollider = typeof(ThrowVacuuming).GetField("colliderPhysics").GetValue(cauldronVaccum) as Collider2D;
            if (target == cauldronCollider)
            {
                return false;
            }
            return true;
        }


    }
}
