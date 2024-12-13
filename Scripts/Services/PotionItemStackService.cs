using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.InteractiveItem.SoundControllers;
using PotionCraft.ObjectBased.InteractiveItem.SoundControllers.Presets;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.Stack.StackItem;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraft.ScriptableObjects.Ingredient;
using PotionCraft.SoundSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using PotionCraftPourBackIn.Scripts.UIElements;
using PotionCraft.NotificationSystem;
using PotionCraftPourBackIn.Scripts.Storage;
using PotionCraft.ManagersSystem.Room;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraftPourBackIn.Scripts.Extensions;
using HarmonyLib;

namespace PotionCraftPourBackIn.Scripts.Services
{
    /// <summary>
    /// Responsible for setting up all of the stack related components for each PotionItem.
    /// Also responsible for patching Stack/StackItem methods to prevent exceptions and provide proper functionality.
    /// </summary>
    public static class PotionItemStackService
    {
        public static void SetupPotionItemForPouringIn(PotionItem potionItem)
        {
            var potion = potionItem.GetInventoryItem() as Potion;
            if (!PotionDataService.PotionHasSerializedData(potion)
                && RecipeService.GetRecipeForPotion(potion) == null)
            {
                return;
            }
            var visualEffect = potionItem.gameObject.AddComponent<StackVisualEffects>();
            var stackScript = potionItem.gameObject.AddComponent<Stack>();
            typeof(MovableItem).GetField("thisRigidbody", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(stackScript, potionItem.GetRigidbody());
            var stackItem = potionItem.gameObject.AddComponent<PotionStackItem>();
            stackItem.potionItem = potionItem;
            typeof(StackItem).GetField("thisRigidbody", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(stackItem, potionItem.GetRigidbody());
            stackItem.transform.parent = potionItem.transform;
            stackScript.itemsFromThisStack = new List<StackItem> { stackItem };
            var potionSoundController = (PotionCraft.ObjectBased.Stack.SoundController)FormatterServices.GetUninitializedObject(typeof(PotionCraft.ObjectBased.Stack.SoundController));
            InitPotionStackSoundController(potionSoundController, stackScript, ((Potion)potionItem.GetInventoryItem()).soundPreset);
            typeof(ItemFromInventory).GetField("_soundController", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(stackScript, potionSoundController);
            stackItem.Initialize(stackScript);
            visualEffect.stackScript = stackScript;
            stackScript.SetInventoryItem(potionItem.GetInventoryItem());
            stackItem.spriteRenderers = [potionItem.visualObject.bottleLiquidMainSpriteRenderer];
            stackItem.graphicStateMachine = potionItem.gameObject.AddComponent<GraphicStateMachine>();
            stackItem.graphicStateMachine.Init(stackItem);
            return;
        }

        private static void InitPotionStackSoundController(PotionCraft.ObjectBased.Stack.SoundController instance, ItemFromInventory itemFromInventory, ItemFromInventoryPreset preset)
        {
            var firstInvIngredient = Managers.Player.Inventory.items.Keys.ToList().OfType<Ingredient>().FirstOrDefault();
            if (firstInvIngredient == null)
            {
                firstInvIngredient = Ingredient.allIngredients.FirstOrDefault();
                if (firstInvIngredient?.soundPreset == null)
                {
                    Plugin.PluginLogger.LogError("Failed to setup sound controller for potion because there were no ingredients in the player inventory.");
                    return;
                }
            }
            //Get the sound preset from the first ingredient we can find so we can use it as a starting point for our PotionStackItem sound controller
            var newPreset = UnityEngine.Object.Instantiate(firstInvIngredient.soundPreset);
            newPreset.hit = preset.hit;

            //SoundController constructor fields
            typeof(PotionCraft.ObjectBased.Stack.SoundController).GetField("stack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, itemFromInventory);
            typeof(PotionCraft.ObjectBased.Stack.SoundController).GetField("soundPresetIngredient", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, newPreset);
            typeof(PotionCraft.ObjectBased.Stack.SoundController).GetField("rubGrindLoopEmitter", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new LoopEmitter(newPreset.rubGrindLoop));

            //ItemFromInventory constructor fields
            typeof(ItemFromInventoryController).GetField("itemFromInventory", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, itemFromInventory);
            typeof(ItemFromInventoryController).GetField("preset", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, preset);
            typeof(ItemFromInventoryController).GetField("rubLoopEmitter", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new LoopEmitter(preset.rubLoop));

            //ItemFromInventory field initializers
            typeof(ItemFromInventoryController).GetField("newCollisions", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new List<Vector2>());
            typeof(ItemFromInventoryController).GetField("collidedWith", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new Dictionary<int, GameObject>());
            typeof(ItemFromInventoryController).GetField("collidedWithToRemove", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new Dictionary<int, Tuple<float, GameObject>>());
        }

        public static void NotifyForPreModPotion(PotionItem instance)
        {
            const float timeBetweenNotifications = 5f;
            //Only show this notification when the potion is grabbed in the laboratory
            if (Managers.Room.CurrentRoomIndex != RoomIndex.Laboratory && Traverse.Create(Managers.Room).Field<RoomIndex>("targetRoom").Value != RoomIndex.Laboratory) return;
            var potion = (Potion)instance.GetInventoryItem();
            if (!PotionDataService.PotionHasSerializedData(potion)
                && RecipeService.GetRecipeForPotion(potion) == null
                && potion != StaticStorage.CurrentPotionCraftPanelPotion)
            {
                //Don't show a notification twice for a potion in same session
                if (StaticStorage.PotionItemsNotifiedFor.Contains(potion)) return;
                if ((Time.unscaledTime - StaticStorage.LastNotifiedTime) < timeBetweenNotifications) return;
                StaticStorage.LastNotifiedTime = Time.unscaledTime;
                StaticStorage.PotionItemsNotifiedFor.Add(potion);
                Plugin.PluginLogger.LogInfo("Grabbed potion does not have recipe data. This is likely a pre-mod potion with no corresponding recipe.");
                Notification.ShowText("This potion is missing critical markings", "You wouldn't know where to start if you poured it back in the cauldron", Notification.TextType.EventText);
            }
        }
    }
}
