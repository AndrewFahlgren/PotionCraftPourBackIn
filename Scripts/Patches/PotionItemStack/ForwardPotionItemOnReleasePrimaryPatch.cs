using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using System.Reflection;
using HarmonyLib;
using PotionCraft.ObjectBased.AlchemyMachine;
using PotionCraft.ObjectBased.ScalesSystem;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(ItemFromInventory), "CustomOnReleasePrimaryCondition")]
    public class ForwardPotionItemOnReleasePrimaryPatch //TODO there are probably a lot of mouse events we need to forward like this: OnGamepadButtonBDowned, OnGamepadButtonBUpped, OnGamepadButtonXDowned, OnGamepadButtonXUpped, OnGamepadButtonYDowned, OnGamepadButtonYUpped
    {
        static void Postfix(ItemFromInventory __instance)
        {
            Ex.RunSafe(() => ForwardPotionItemOnReleasePrimary(__instance));
        }

        private static void ForwardPotionItemOnReleasePrimary(ItemFromInventory instance)
        {
            if (instance is not PotionItem) return;
            var stack = instance.gameObject.GetComponent<Stack>();
            if (stack == null) return;
            //Ensure we are not forwarding events when interacting with scales or alchemy machine
            if (Managers.Cursor.hoveredInteractiveItem is ScalesCupDisplay || Managers.Cursor.hoveredInteractiveItem is AlchemyMachineSlot) return;
            typeof(Stack).GetMethod("CustomOnReleasePrimaryCondition", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(stack, null);
        }
    }
}
