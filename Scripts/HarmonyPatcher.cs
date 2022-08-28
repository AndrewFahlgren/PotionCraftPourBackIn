using HarmonyLib;
using PotionCraft;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ManagersSystem.SaveLoad;
using PotionCraft.Npc;
using PotionCraft.ObjectBased.AlchemyMachine;
using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.RecipeMap;
using PotionCraft.ObjectBased.RecipeMap.Buttons;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PathMapItem;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.Stack.StackItem;
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
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using static PotionCraft.Utils.Dictionaries;
using SoundController = PotionCraft.ObjectBased.Stack.StackItem.SoundController;

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
        static bool Prefix(PotionItem __instance, Collider2D target, bool ignore)
        {
            return Ex.RunSafe(() => CauldronInteractionService.AllowCauldronCollision(__instance, target, ignore));
        }
    }

    [HarmonyPatch(typeof(StackVisualEffects), "SpawnEffectsUpdate")]
    public class HidePotionStackVisualEffectsPatch
    {
        static bool Prefix(StackVisualEffects __instance)
        {
            return Ex.RunSafe(() => !PotionService.IsPotionStackItemEffect(__instance));
        }
    }

    [HarmonyPatch(typeof(StackVisualEffects))]
    public class HidePotionStackExplosionEffectsPatch
    {
        static MethodInfo TargetMethod()
        {
            return typeof(StackVisualEffects).GetMethod("SpawnEffectsExplosion", new[] { typeof(VisualEffectScriptableObject), typeof(Vector3), typeof(SpriteSortingLayers) });
        }

        static bool Prefix(StackVisualEffects __instance)
        {
            return Ex.RunSafe(() => !PotionService.IsPotionStackItemEffect(__instance));
        }
    }

    [HarmonyPatch(typeof(SoundController), ".ctor")]
    public class OverridePotionStackItemSoundControllerPatch
    {
        static bool Prefix(SoundController __instance, StackItem stackItem)
        {
            return Ex.RunSafe(() => CauldronInteractionService.OverridePotionStackItemSoundController(__instance, stackItem), () => OnError(stackItem));
        }

        private static bool OnError(StackItem stackItem)
        {
            try
            {
                if (stackItem is PotionStackItem) return false;
            }
            catch(Exception ex)
            {
                Ex.LogException(ex);
            }
            return true;
        }
    }

    //[HarmonyPatch(typeof(WaterZone), "OnTriggerStay2D")]
    //public class AcceptPotionCollisionPatch
    //{
    //    static bool Prefix(Collider2D other)
    //    {
    //        return Ex.RunSafe(() => CauldronInteractionService.AcceptPotionCollision(other));
    //    }
    //}



}
