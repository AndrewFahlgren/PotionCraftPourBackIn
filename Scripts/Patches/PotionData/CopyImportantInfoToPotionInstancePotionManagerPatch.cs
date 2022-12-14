using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraftPourBackIn.Scripts.Storage;
using static PotionCraft.SaveLoadSystem.ProgressState;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(PotionManager), "GeneratePotionFromCurrentPotion")]
    public class CopyImportantInfoToPotionInstancePotionManagerPatch
    {
        [HarmonyAfter(new string[] { "com.fahlgorithm.potioncraftalchemymachinerecipies" })]
        static void Postfix(Potion __result)
        {
            Ex.RunSafe(() => CopyImportantInfoToPotionInstance(__result));
        }

        private static void CopyImportantInfoToPotionInstance(Potion copyTo)
        {
            if (!StaticStorage.CurrentlyMakingPotion || StaticStorage.CurrentlyGeneratingRecipe) return;

            var copyFrom = SerializedPotionFromPanel.GetPotionFromCurrentPotion();
            var copyFromPotion = Managers.Potion.potionCraftPanel.GetCurrentPotion();
            PotionItem linkedPotionItem = null;
            if (copyFromPotion == null)
            {
                if (Managers.Cursor.grabbedInteractiveItem is PotionItem potionItem)
                {
                    potionItem.inventoryItem = copyTo;
                    linkedPotionItem = potionItem;
                }
            }
            PotionDataService.CopyImportantInfoToPotionInstance(copyTo, copyFromPotion, copyFrom);
            if (linkedPotionItem != null) PotionItemStackService.SetupPotionItemForPouringIn(linkedPotionItem);
        }
    }
}
