using HarmonyLib;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraftPourBackIn.Scripts.Storage;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(SavePotionRecipeButton), "GenerateRecipe")]
    public class SetCurrentlyGeneratingRecipePatch
    {
        static bool Prefix()
        {
            return Ex.RunSafe(() => SetCurrentlyGeneratingRecipe(true));
        }

        static void Postfix()
        {
            Ex.RunSafe(() => SetCurrentlyGeneratingRecipe(false));
        }

        private static bool SetCurrentlyGeneratingRecipe(bool currentlyGeneratingRecipe)
        {
            StaticStorage.CurrentlyGeneratingRecipe = currentlyGeneratingRecipe;
            return true;
        }
    }
}
