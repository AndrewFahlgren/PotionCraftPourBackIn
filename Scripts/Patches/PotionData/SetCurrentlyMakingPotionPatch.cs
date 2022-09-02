using HarmonyLib;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraftPourBackIn.Scripts.Storage;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(PotionCraftPanel), "MakePotion")]
    public class SetCurrentlyMakingPotionPatch
    {
        static bool Prefix()
        {
            return Ex.RunSafe(() => SetCurrentlyMakingPotion(true));
        }

        static void Postfix()
        {
            Ex.RunSafe(() => SetCurrentlyMakingPotion(false));
        }

        private static bool SetCurrentlyMakingPotion(bool currentlyMakingPotion)
        {
            StaticStorage.CurrentlyMakingPotion = currentlyMakingPotion;
            return true;
        }
    }
}
