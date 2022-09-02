﻿using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.InteractiveItem.InventoryObject;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.ScriptableObjects;
using PotionCraftPourBackIn.Scripts.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PotionCraftPourBackIn.Scripts.Patchers
{
    public static class UnfinishedPotionPatcher
    {
        [HarmonyPatch(typeof(PotionCraftPanel), "UpdatePotionInCraftPanel")]
        public class EnableFinishPotionButtonForUnfinishedPotionsPatch
        {
            static void Postfix(PotionCraftPanel __instance)
            {
                Ex.RunSafe(() => EnableFinishPotionButtonForUnfinishedPotions(__instance));
            }
        }

        public static void EnableFinishPotionButtonForUnfinishedPotions(PotionCraftPanel instance)
        {
            if (!instance.IsPotionBrewingStarted()) return;
            var limitedInventoryPanelButtons = typeof(ItemsPanel).GetField("buttons", BindingFlags.NonPublic | BindingFlags.Instance)
                                                                 .GetValue(instance.limitedInventoryPanel) as List<InventoryObject>;
            limitedInventoryPanelButtons.First().Locked = false;
            instance.potionFinishingButton.Locked = false;
        }


        [HarmonyPatch(typeof(PotionInventoryObject), "CanBeInteractedNow")]
        public class EnableGrabUnfinishedPotionToFinishPatch
        {
            static bool Prefix(InventoryItem ___inventoryItem)
            {
                return Ex.RunSafe(() => EnableGrabUnfinishedPotionToFinishPrefix(___inventoryItem));
            }
            static void Postfix(InventoryItem ___inventoryItem)
            {
                Ex.RunSafe(() => EnableGrabUnfinishedPotionToFinishPostfix(___inventoryItem));
            }
        }

        private static bool EnableGrabUnfinishedPotionToFinishPrefix(InventoryItem inventoryItem)
        {
            if (inventoryItem is not Potion potion) return true;
            if (!Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return true;
            if (potion.Effects.Any()) return true;
            potion.Effects = new[] { PotionEffect.allPotionEffects.First() };
            StaticStorage.DummyEffectAddedToEnableFinishPotionGrab = true;
            return true;
        }

        private static void EnableGrabUnfinishedPotionToFinishPostfix(InventoryItem inventoryItem)
        {
            if (inventoryItem is not Potion potion) return;
            if (!Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return;
            if (!StaticStorage.DummyEffectAddedToEnableFinishPotionGrab) return;
            potion.Effects = new PotionEffect[0];
            StaticStorage.DummyEffectAddedToEnableFinishPotionGrab = false;
        }
    }
}
