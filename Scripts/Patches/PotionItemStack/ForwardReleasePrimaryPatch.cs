using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using HarmonyLib;
using PotionCraft.ObjectBased.ScalesSystem;
using PotionCraft.ObjectBased.AlchemyMachine;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(ItemFromInventory), "OnReleasePrimary")]
    public class ForwardReleasePrimaryPatch
    {
        static void Postfix(ItemFromInventory __instance)
        {
            Ex.RunSafe(() => ForwardReleasePrimary(__instance));
        }

        private static void ForwardReleasePrimary(ItemFromInventory instance)
        {
            if (instance == null) return;
            if (instance.markedAsDestroyed) return;
            if (instance is not PotionItem potionItem) return;
            var stack = potionItem.GetComponent<Stack>();
            if (stack == null) return;
            //Ensure we are not forwarding events when interacting with scales or alchemy machine
            if (Managers.Cursor.hoveredInteractiveItem is ScalesCupDisplay || Managers.Cursor.hoveredInteractiveItem is AlchemyMachineSlot) return;
            stack.OnReleasePrimary();
        }
    }
}
