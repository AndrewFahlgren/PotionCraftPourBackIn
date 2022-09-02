using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using System.Linq;
using HarmonyLib;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(RecipeBook), "OnLoad")]
    public class SetupPotionItemsForPouringInOnRecipeLoadPatch
    {
        static void Postfix()
        {
            Ex.RunSafe(() => SetupPotionItemsForPouringInOnRecipeLoad());
        }

        private static void SetupPotionItemsForPouringInOnRecipeLoad()
        {
            Managers.Game.ItemContainer.GetComponentsInChildren<PotionItem>().Where(p => p.GetComponent<Stack>() == null).ToList().ForEach(PotionItemStackService.SetupPotionItemForPouringIn);
        }
    }
}
