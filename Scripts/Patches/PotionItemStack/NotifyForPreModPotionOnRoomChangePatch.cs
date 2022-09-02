using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Potion;
using HarmonyLib;
using PotionCraft.ManagersSystem.Room;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(RoomManager), "GoTo")]
    public class NotifyForPreModPotionOnRoomChangePatch
    {
        static void Postfix()
        {
            Ex.RunSafe(() => NotifyForPreModPotionOnRoomChange());
        }

        private static void NotifyForPreModPotionOnRoomChange()
        {
            if (Managers.Cursor.grabbedInteractiveItem is not PotionItem potionItem) return;
            PotionItemStackService.NotifyForPreModPotion(potionItem);
        }
    }
}
