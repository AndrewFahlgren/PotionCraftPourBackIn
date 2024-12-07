using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Stack.StackItem;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraft.ScriptableObjects.Ingredient;
using PotionCraft.SoundSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SoundController = PotionCraft.ObjectBased.Stack.StackItem.SoundController;
using HarmonyLib;
using PotionCraftPourBackIn.Scripts.UIElements;
using PotionCraft.ScriptableObjects;

namespace PotionCraftPourBackIn.Scripts.Patches
{
    [HarmonyPatch(typeof(SoundController), MethodType.Constructor, new[] { typeof(StackItem) })]
    public class OverridePotionStackItemSoundControllerPatch
    {
        static bool Prefix(SoundController __instance, StackItem stackItem)
        {
            return Ex.RunSafe(() => OverridePotionStackItemSoundController(__instance, stackItem), () => OnError(stackItem));
        }

        private static bool OnError(StackItem stackItem)
        {
            try
            {
                if (stackItem is PotionStackItem) return false;
            }
            catch (Exception ex)
            {
                Ex.LogException(ex);
            }
            return true;
        }

        private static bool OverridePotionStackItemSoundController(SoundController instance, StackItem stackItem)
        {
            if (stackItem is not PotionStackItem) return true;
            var potionItem = ((PotionStackItem)stackItem).potionItem;
            typeof(SoundController).GetField("stackItem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, stackItem);
            typeof(SoundController).GetField("newCollisions", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new List<Vector2>());
            typeof(SoundController).GetField("collidedWith", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new List<GameObject>());
            var firstInvIngredient = Managers.Player.Inventory.items.Keys.ToList().OfType<Ingredient>().FirstOrDefault();
            if (firstInvIngredient == null)
            {
                firstInvIngredient = Ingredient.allIngredients.FirstOrDefault();
                if (firstInvIngredient == null)
                {
                    Plugin.PluginLogger.LogError("Failed to setup sound controller for potion because there were no ingredients in the player inventory.");
                    return false;
                }
            }
            //Get the sound preset from the first ingredient we can find so we can use it as a starting point for our PotionStackItem sound controller
            var preset = UnityEngine.Object.Instantiate(firstInvIngredient.soundPreset);
            var potionSoundPreset = ((Potion)Traverse.Create(potionItem).Property<InventoryItem>("InventoryItem").Value).soundPreset;
            preset.hit = potionSoundPreset.hit;
            typeof(SoundController).GetField("preset", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, preset);
            typeof(SoundController).GetField("rubLoopEmitter", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new LoopEmitter(preset.rubLoop));
            return false;
        }
    }
}
