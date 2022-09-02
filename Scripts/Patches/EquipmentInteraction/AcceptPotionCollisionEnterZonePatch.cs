using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using UnityEngine;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(EnterZone), "OnTriggerStay2D")]
    public class AcceptPotionCollisionEnterZonePatch
    {
        static bool Prefix(Collider2D other, ThrowVacuuming ___throwVacuuming)
        {
            return Ex.RunSafe(() => EquipmentInteractionService.DisallowMortarPotionCollisions(other, ___throwVacuuming));
        }
    }
}
