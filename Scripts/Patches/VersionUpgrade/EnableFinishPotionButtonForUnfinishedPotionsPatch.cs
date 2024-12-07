using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.InteractiveItem.InventoryObject;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    public class ConvertPotionInventoryItemV1toV2Pacth
    {
        [HarmonyPatch("PotionCraft.Assemblies.ConverterFrom110To200.SerializedInventorySlot", "ConvertPotionAndLegendarySubstancesToNew")]
        public class SerializedInventorySlot_ConvertPotionAndLegendarySubstancesToNew
        {
            static void Postfix(ref object __result, string ___data, string ___classFullName)
            {
                var myResult = __result;
                Ex.RunSafe(() => ConvertPotionInventoryItem(myResult, ___data, ___classFullName));
            }

            private static void ConvertPotionInventoryItem(object result, string unconvertedJson, string classFullName)
            {
                var traverse = Traverse.Create(result);
                var convertedJson = traverse.Field<string>("data").Value;
                var convertedJObj = JObject.Parse(convertedJson);
                var potionFromPanelJObj = JObject.Parse(unconvertedJson)["potionFromPanel"];
                var potionFromPanelJson = potionFromPanelJObj.ToString(Formatting.None);
                //deserialize into SerializedPotionRecipeDataOld,
                var potionRecipeDataOld = JsonConvert.DeserializeObject(potionFromPanelJson, AccessTools.TypeByName("PotionCraft.Assemblies.ConverterFrom110To200.SerializedPotionRecipeDataOld"));
                //convert,
                var potionRecipeDataNew = Traverse.Create(potionRecipeDataOld).Method("ConvertPotionToNew").GetValue();
                //serialize,
                var potionRecipeDataJson = JsonConvert.SerializeObject(potionRecipeDataNew);
                //insert into convertedjsonObj,
                var potionRecipeDataJObj = JObject.Parse(potionRecipeDataJson);
                convertedJObj["recipeData"] = potionRecipeDataJObj;
                //reserialize converted json and save to data field in result
                var convertedWithRecipeData = convertedJObj.ToString(Formatting.None);
                traverse.Field("data").SetValue(convertedWithRecipeData);
            }
        }
    }
}
