using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using UnityEngine;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(EnterZone), "OnTriggerExit2D")]
    public class OverrideEnterZoneOnTriggerExit2DPatch
    {
        static bool Prefix(Collider2D other, ThrowVacuuming ___throwVacuuming)
        {
            return Ex.RunSafe(() => EquipmentInteractionService.DisallowMortarPotionCollisions(other, ___throwVacuuming));
        }
    }
}
