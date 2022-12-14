using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.Services;
using PotionCraft.Assemblies.GamepadNavigation.Conditions;
using System.Reflection;

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
            var setConditionValue = typeof(ConditionValue).GetMethod("SetConditionValue", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Condition), typeof(bool) }, new ParameterModifier[0]);
            if (setConditionValue == null)
            {
                return;
            }
            setConditionValue.Invoke(null, new object[] { Condition.PotionInHand, true });
        }
    }
}
