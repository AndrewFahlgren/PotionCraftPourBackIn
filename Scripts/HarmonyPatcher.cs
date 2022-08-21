using HarmonyLib;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ManagersSystem.SaveLoad;
using PotionCraft.ObjectBased.AlchemyMachine;
using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.RecipeMap;
using PotionCraft.ObjectBased.RecipeMap.Buttons;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PathMapItem;
using PotionCraft.ObjectBased.UIElements;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ObjectBased.UIElements.FinishLegendarySubstanceMenu;
using PotionCraft.ObjectBased.UIElements.Tooltip;
using PotionCraft.SaveFileSystem;
using PotionCraft.SaveLoadSystem;
using PotionCraft.ScriptableObjects;
using PotionCraftPourBackIn.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace PotionCraftPourBackIn.Scripts
{
    [HarmonyPatch(typeof(Potion), "Clone")]
    public class CopyImportantInfoToPotionInstancePotionPatch
    {
        static void Postfix(ref Potion __result, Potion __instance)
        {
            RunSafe(__result, __instance);
        }

        private static void RunSafe(Potion __result, Potion __instance)
        {
            Ex.RunSafe(() => PotionService.CopyImportantInfoToPotionInstance(__result, __instance));
        }
    }


    [HarmonyPatch(typeof(PotionManager), "GeneratePotionFromCurrentPotion")]
    public class CopyImportantInfoToPotionInstancePotionManagerPatch
    {
        static void Postfix(ref Potion __result, PotionManager __instance)
        {
            RunSafe(__result, __instance);
        }

        private static void RunSafe(Potion __result, PotionManager __instance)
        {
            Ex.RunSafe(() => PotionService.CopyImportantInfoToPotionInstance(__result, __instance));
        }
    }

    [HarmonyPatch(typeof(PotionItem), "IgnoreCollision")]
    public class AllowCauldronCollisionPatch
    {
        static bool Prefix(Collider2D target, bool ignore)
        {
            return Ex.RunSafe(() => CauldronInteractionService.AllowCauldronCollision(target, ignore));
        }
    }

    [HarmonyPatch(typeof(WaterZone), "OnTriggerStay2D")]
    public class AcceptPotionCollisionPatch
    {
        static bool Prefix(Collider2D other)
        {
            return Ex.RunSafe(() => CauldronInteractionService.AcceptPotionCollision(other));
        }
    }
}
