using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(PotionItem), "OnGrabPrimary")]
    public class ForwardGrabPrimaryPatch
    {
        static void Postfix(PotionItem __instance)
        {
            Ex.RunSafe(() => ForwardGrabPrimary(__instance));
        }

        private static void ForwardGrabPrimary(PotionItem instance)
        {
            var stack = instance.GetComponent<Stack>();
            if (stack == null)
            {
                PotionItemStackService.NotifyForPreModPotion(instance);
                return;
            }
            stack.OnGrabPrimary();
        }
    }
}
