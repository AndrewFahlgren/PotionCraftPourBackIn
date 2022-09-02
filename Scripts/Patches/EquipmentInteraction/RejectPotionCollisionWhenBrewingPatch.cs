using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using PotionCraft.ObjectBased.Potion;
using UnityEngine;
using HarmonyLib;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(WaterZone), "OnTriggerStay2D")]
    public class RejectPotionCollisionWhenBrewingPatch
    {
        static bool Prefix(Collider2D other)
        {
            return Ex.RunSafe(() => RejectPotionCollisionWhenBrewing(other));
        }

        private static bool RejectPotionCollisionWhenBrewing(Collider2D other)
        {
            var potionItem = other.GetComponentInParent<PotionItem>();
            if (potionItem == null) return true;

            if (Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return false;

            return true;
        }
    }
}
