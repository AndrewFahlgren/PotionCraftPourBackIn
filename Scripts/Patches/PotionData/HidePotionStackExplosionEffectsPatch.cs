using HarmonyLib;
using PotionCraft;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ScriptableObjects;
using System.Reflection;
using UnityEngine;
using PotionCraftPourBackIn.Scripts.Services;
using IngredientVisualEffectSystem;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(StackVisualEffects))]
    public class HidePotionStackExplosionEffectsPatch
    {
        static MethodInfo TargetMethod()
        {
            return typeof(StackVisualEffects).GetMethod("SpawnEffectsExplosion", new[] { typeof(IngredientVisualEffect), typeof(Vector3), typeof(SpriteSortingLayers) });
        }

        static bool Prefix(StackVisualEffects __instance)
        {
            return Ex.RunSafe(() => !PotionDataService.IsPotionStackItemEffect(__instance));
        }
    }
}
