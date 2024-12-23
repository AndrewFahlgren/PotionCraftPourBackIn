﻿using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraftPourBackIn.Scripts.Storage;
using static PotionCraft.SaveLoadSystem.ProgressState;
using PotionCraftPourBackIn.Scripts.Services;
using PotionCraft.ScriptableObjects;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(PotionManager), "GeneratePotionFromCurrentPotion")]
    public class CopyImportantInfoToPotionInstancePotionManagerPatch
    {
        [HarmonyAfter(["com.fahlgorithm.potioncraftalchemymachinerecipies"])]
        static void Postfix(Potion __result)
        {
            Ex.RunSafe(() => CopyImportantInfoToPotionInstance(__result));
        }

        private static void CopyImportantInfoToPotionInstance(Potion copyTo)
        {
            if (!StaticStorage.CurrentlyMakingPotion || StaticStorage.CurrentlyGeneratingRecipe) return;

            var copyFrom = Managers.Potion.GetSerializedPotionRecipeDataFromCurrentPotion();
            var copyFromPotion = Managers.Potion.potionCraftPanel.GetRecipeBookPageContent() as Potion;
            PotionItem linkedPotionItem = null;
            if (copyFromPotion == null)
            {
                if (Managers.Cursor.grabbedInteractiveItem is PotionItem potionItem)
                {
                    Traverse.Create(potionItem).Property("InventoryItem").SetValue(copyTo);
                    linkedPotionItem = potionItem;
                }
            }
            PotionDataService.CopyImportantInfoToPotionInstance(copyTo, copyFromPotion, copyFrom);
            if (linkedPotionItem != null)
            {
                PotionItemStackService.SetupPotionItemForPouringIn(linkedPotionItem);
                //When grabbing from the potion craft panel we have to setup the potion stack item after grab has already happened
                //Call this again so we can forward the even to the stack/stackitem
                linkedPotionItem.OnGrabPrimary();
            }
        }
    }
}
