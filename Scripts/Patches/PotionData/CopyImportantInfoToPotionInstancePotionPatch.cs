﻿using HarmonyLib;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(Potion), "Clone")]
    public class CopyImportantInfoToPotionInstancePotionPatch
    {
        static void Postfix(Potion __result, Potion __instance)
        {
            Ex.RunSafe(() => CopyImportantInfoToPotionInstance(__result, __instance));
        }

        /// <summary>
        /// This method copies important information to the potion that is normally lost unless the potion is saved as a recipe
        /// This is important so we can use the potion just like a recipe later
        /// </summary>
        private static void CopyImportantInfoToPotionInstance(Potion copyTo, Potion copyFrom)
        {
            PotionDataService.CopyImportantInfoToPotionInstance(copyTo, copyFrom, copyFrom.potionFromPanel);
        }
    }
}
