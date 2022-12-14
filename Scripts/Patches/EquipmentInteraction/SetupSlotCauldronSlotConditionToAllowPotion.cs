using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using UnityEngine;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.Services;
using PotionCraft.Assemblies.GamepadNavigation;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    public class SetupSlotCauldronSlotConditionToAllowPotion
    {
        [HarmonyPatch(typeof(Slot), "SetupConditions")]
        public class Slot_SetupConditions
        {
            static bool Prefix(Slot __instance)
            {
                return Ex.RunSafe(() => EquipmentInteractionService.SetupSlotCauldronSlotConditionToAllowPotion(__instance));
            }

        }
    }
}
