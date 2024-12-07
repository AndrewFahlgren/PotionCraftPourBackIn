using HarmonyLib;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.InteractiveItem.InventoryObject;
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

        public static InventoryItem GetInventoryItem(this InventoryObject item)
        {
            return Traverse.Create(item).Property<InventoryItem>("InventoryItem").Value;
        }
        public static void SetInventoryItem(this InventoryObject item, InventoryItem value)
        {
            Traverse.Create(item).Property("InventoryItem").SetValue(value);
        }
    }
}
