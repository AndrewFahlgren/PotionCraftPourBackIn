using HarmonyLib;
using PotionCraft.ObjectBased;
using PotionCraft.ScriptableObjects;
using PotionCraftPourBackIn.Scripts.UIElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace PotionCraftPourBackIn.Scripts.Extensions
{
    public static class PotionExtensions
    {
        public static InventoryItem GetInventoryItem(this ItemFromInventory item)
        {
            return Traverse.Create(item).Property<InventoryItem>("InventoryItem").Value;
        }
        public static void SetInventoryItem(this ItemFromInventory item, InventoryItem value)
        {
            Traverse.Create(item).Property("InventoryItem").SetValue(value);
        }
    }
}
