using HarmonyLib;
using PotionCraft.SaveLoadSystem;
using PotionCraft.ScriptableObjects;
using UnityEngine;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(Potion), "GetSerializedInventorySlot")]
    public class SavePotionSerializedDataPatch
    {
        static void Postfix(SerializedInventorySlot __result, Potion __instance)
        {
            Ex.RunSafe(() => SavePotionSerializedData(__result, __instance));
        }

        private static void SavePotionSerializedData(SerializedInventorySlot result, Potion instance)
        {
            var dataToInsert = $",\"potionFromPanel\":{JsonUtility.ToJson(instance.potionFromPanel)}";
            var insertIndex = result.data.LastIndexOf('}');
            result.data = result.data.Insert(insertIndex, dataToInsert);
        }
    }
}
