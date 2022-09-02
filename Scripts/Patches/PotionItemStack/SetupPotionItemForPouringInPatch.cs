using PotionCraft.ObjectBased.Potion;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(PotionItem), "SpawnNewPotion")]
    public class SetupPotionItemForPouringInPatch
    {
        static void Postfix(PotionItem __result)
        {
            Ex.RunSafe(() => PotionItemStackService.SetupPotionItemForPouringIn(__result));
        }
    }
}
