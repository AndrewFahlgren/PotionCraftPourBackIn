using HarmonyLib;
using PotionCraft.Assemblies.GamepadNavigation.Conditions;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack.StackItem;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraftPourBackIn.Scripts.Services;
using PotionCraftPourBackIn.Scripts.UIElements;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(LedgeController), "DetachItem")]
    public class ForwardLedgeControllerDetachItemToPotionStackItemPatch
    {
        static void Postfix(LedgeController __instance, ILedgeTarget ledgeTarget, bool removeFromColliders)
        {
            Ex.RunSafe(() => ForwardLedgeControllerDetachItemToPotionStackItem(__instance, ledgeTarget, removeFromColliders));
        }

        private static void ForwardLedgeControllerDetachItemToPotionStackItem(LedgeController __instance, ILedgeTarget ledgeTarget, bool removeFromColliders)
        {
            if (ledgeTarget is not PotionItem potionItem) return;

            var stackLedgeTarget = potionItem.GetComponent<PotionStackItem>();
            __instance.DetachItem(stackLedgeTarget, removeFromColliders);
        }
    }

    [HarmonyPatch(typeof(LedgeController), "AttachItem")]
    public class ForwardLedgeControllerAttachItemToPotionStackItemPatch
    {
        static void Postfix(LedgeController __instance, ILedgeTarget ledgeTarget)
        {
            Ex.RunSafe(() => ForwardLedgeControllerAttachItemToPotionStackItem(__instance, ledgeTarget));
        }

        private static void ForwardLedgeControllerAttachItemToPotionStackItem(LedgeController __instance, ILedgeTarget ledgeTarget)
        {
            if (ledgeTarget is not PotionItem potionItem) return;

            var stackLedgeTarget = potionItem.GetComponent<PotionStackItem>();
            __instance.AttachItem(stackLedgeTarget);
        }
    }
}
