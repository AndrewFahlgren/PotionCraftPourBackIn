using HarmonyLib;
using PotionCraft.InventorySystem;
using PotionCraft.SaveLoadSystem;
using PotionCraft.ScriptableObjects.Potion;
using System.Reflection;
using UnityEngine;
using static PotionCraft.SaveLoadSystem.ProgressState;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(Potion))]
    public class LoadPotionSerializedDataPatch
    {
        static MethodInfo TargetMethod()
        {
            return typeof(Potion).GetMethod("GetFromSerializedObject", new[] { typeof(SerializedInventorySlot) });
        }

        static void Postfix(Potion __result, SerializedInventorySlot serializedObject)
        {
            Ex.RunSafe(() => LoadPotionSerializedData(__result, serializedObject));
        }

        private static void LoadPotionSerializedData(Potion result, SerializedInventorySlot serializedRecipe)
        {
            //Check if there is an existing potionFromPanel
            var keyIndex = serializedRecipe.data.IndexOf("recipeData");
            if (keyIndex == -1)
            {
                return;
            }
            //Determine the start of the object
            var startPotionFromPanelIndex = serializedRecipe.data.IndexOf('{', keyIndex);
            if (startPotionFromPanelIndex == -1)
            {
                Plugin.PluginLogger.LogInfo("Error: recipeData data in serialized potion is malformed.");
                return;
            }
            //Find the closing bracket of the list
            var endPotionFromPanelIndex = GetEndJsonIndex(serializedRecipe.data, startPotionFromPanelIndex, false);
            if (endPotionFromPanelIndex >= serializedRecipe.data.Length)
            {
                Plugin.PluginLogger.LogInfo("Error: recipeData data in serialized potion is malformed (bad end index).");
                return;
            }

            var savedPotionFromPanelJson = serializedRecipe.data.Substring(startPotionFromPanelIndex, endPotionFromPanelIndex - startPotionFromPanelIndex);
            if (savedPotionFromPanelJson.Length <= 2)
            {
                Plugin.PluginLogger.LogInfo("Error: recipeData data in serialized potion is malformed (empty object).");
                return;
            }

            result.SetSerializedPotionFromPanel(JsonUtility.FromJson<SerializedPotionRecipeData>(savedPotionFromPanelJson));
        }

        /// <summary>
        /// Manually parses the json to find the closing bracket for this json object.
        /// </summary>
        /// <param name="input">the json string to parse.</param>
        /// <param name="startIndex">the openning bracket of this object/list.</param>
        /// <param name="useBrackets">if true this code will look for closing brackets and if false this code will look for curly braces.</param>
        /// <returns></returns>
        private static int GetEndJsonIndex(string input, int startIndex, bool useBrackets)
        {
            var endIndex = startIndex + 1;
            var unclosedCount = 1;
            var openChar = useBrackets ? '[' : '{';
            var closeChar = useBrackets ? ']' : '}';
            while (unclosedCount > 0 && endIndex < input.Length - 1)
            {
                var nextOpenIndex = input.IndexOf(openChar, endIndex);
                var nextCloseIndex = input.IndexOf(closeChar, endIndex);
                if (nextOpenIndex == -1 && nextCloseIndex == -1)
                {
                    break;
                }
                if (nextOpenIndex == -1) nextOpenIndex = int.MaxValue;
                if (nextCloseIndex == -1) nextCloseIndex = int.MaxValue;
                if (nextOpenIndex < nextCloseIndex)
                {
                    endIndex = nextOpenIndex + 1;
                    unclosedCount++;
                }
                else if (nextCloseIndex < nextOpenIndex)
                {
                    endIndex = nextCloseIndex + 1;
                    unclosedCount--;
                }
            }
            return endIndex;
        }
    }
}
