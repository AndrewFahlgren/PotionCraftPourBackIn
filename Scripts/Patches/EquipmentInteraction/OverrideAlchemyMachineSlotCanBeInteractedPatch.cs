﻿using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Potion;
using HarmonyLib;
using PotionCraft.ObjectBased.AlchemyMachine;
using PotionCraft.ScriptableObjects.Potion;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(AlchemyMachineSlot), "CanBeInteractedNow")]
    public class OverrideAlchemyMachineSlotCanBeInteractedPatch
    {
        static void Postfix(ref bool __result)
        {
            OverrideAlchemyMachineSlotCanBeInteracted(ref __result);
        }

        private static void OverrideAlchemyMachineSlotCanBeInteracted(ref bool result)
        {
            if (result == false) return;
            var newResult = true;
            Ex.RunSafe(() =>
            {
                var grabbedInteractiveItem = Managers.Cursor.grabbedInteractiveItem;
                if (grabbedInteractiveItem is not PotionItem potionItem) return;
                var potion = (potionItem.inventoryItem as Potion);
                if (potion.Effects.Length == 0 || potion.Effects[0] == null) newResult = false;
            });
            if (!newResult) result = false;
        }
    }
}
