using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Potion;
using HarmonyLib;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraft.ScriptableObjects;
using PotionCraft.ObjectBased.AlchemyMachine;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(AlchemyMachineSlotObject), "CanBeInteractedNow")]
    public class OverrideAlchemyMachineSlotCanBeInteractedPatch
    {
        static void Postfix(ref bool __result)
        {
            OverrideAlchemyMachineSlotCanBeInteracted(ref __result);
        }

        private static void OverrideAlchemyMachineSlotCanBeInteracted(ref bool result)
        {
            if (result == false) return;
            var newResult = true;
            Ex.RunSafe(() =>
            {
                var grabbedInteractiveItem = Managers.Cursor.grabbedInteractiveItem;
                if (grabbedInteractiveItem is not PotionItem potionItem) return;
                var potion = Traverse.Create(potionItem).Property<InventoryItem>("InventoryItem").Value as Potion;
                if (potion.Effects.Length == 0 || potion.Effects[0] == null) newResult = false;
            });
            if (!newResult) result = false;
        }
    }
}
