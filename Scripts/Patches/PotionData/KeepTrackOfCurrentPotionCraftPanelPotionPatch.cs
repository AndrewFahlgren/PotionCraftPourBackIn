using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraftPourBackIn.Scripts.Storage;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(PotionCraftPanel), "UpdatePotionInCraftPanel")]
    public class KeepTrackOfCurrentPotionCraftPanelPotionPatch
    {
        static void Postfix()
        {
            Ex.RunSafe(() => KeepTrackOfCurrentPotionCraftPanelPotion());
        }

        private static void KeepTrackOfCurrentPotionCraftPanelPotion()
        {
            StaticStorage.CurrentPotionCraftPanelPotion = (Potion)Managers.Potion.potionCraftPanel.GetRecipeBookPageContent();
        }
    }
}
