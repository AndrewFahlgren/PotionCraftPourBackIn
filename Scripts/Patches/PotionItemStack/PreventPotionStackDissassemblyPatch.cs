using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using HarmonyLib;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(Stack), "DisassembleStack")]
    public class PreventPotionStackDissassemblyPatch
    {
        static bool Prefix(Stack __instance)
        {
            return Ex.RunSafe(() => PreventPotionStackDissassembly(__instance));
        }

        private static bool PreventPotionStackDissassembly(Stack instance)
        {
            return instance.GetComponent<PotionItem>() == null;
        }
    }
}
