using PotionCraft.ObjectBased.Stack.StackItem;
using UnityEngine;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(GraphicStateMachine), "OnTriggerEnter2D")]
    public class DisallowMortarGraphicStateSwitchPatch
    {
        static bool Prefix(GraphicStateMachine __instance, Collider2D other)
        {
            return Ex.RunSafe(() => EquipmentInteractionService.DisallowMortarGraphicStateSwitch(__instance, other));
        }
    }
}
