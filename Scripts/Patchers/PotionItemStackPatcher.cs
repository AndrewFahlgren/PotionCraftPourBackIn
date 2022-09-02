using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.InteractiveItem.SoundControllers;
using PotionCraft.ObjectBased.InteractiveItem.SoundControllers.Presets;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.Stack.StackItem;
using PotionCraft.ScriptableObjects;
using PotionCraft.ScriptableObjects.Ingredient;
using PotionCraft.SoundSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using SoundController = PotionCraft.ObjectBased.Stack.StackItem.SoundController;
using HarmonyLib;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraftPourBackIn.Scripts.UIElements;
using PotionCraftPourBackIn.Scripts.Services;

namespace PotionCraftPourBackIn.Scripts.Patchers
{
    /// <summary>
    /// Responsible for setting up all of the stack related components for each PotionItem.
    /// Also responsible for patching Stack/StackItem methods to prevent exceptions and provide proper functionality.
    /// </summary>
    public static class PotionItemStackPatcher
    {
        #region PotionItem Setup

        [HarmonyPatch(typeof(PotionItem), "SpawnNewPotion")]
        public class SetupPotionItemForPouringInPatch
        {
            static void Postfix(PotionItem __result)
            {
                Ex.RunSafe(() => SetupPotionItemForPouringIn(__result));
            }
        }

        public static void SetupPotionItemForPouringIn(PotionItem potionItem)
        {
            var potion = potionItem.inventoryItem as Potion;
            if (potion.potionFromPanel.serializedPath.indicatorTargetPosition == Vector2.zero
                && RecipeService.GetRecipeForPotion(potion) == null)
            {
                return;
            }
            var visualEffect = potionItem.gameObject.AddComponent<StackVisualEffects>();
            var stackScript = potionItem.gameObject.AddComponent<Stack>();
            typeof(Stack).GetField("thisRigidbody", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(stackScript, potionItem.GetRigidbody());
            var stackItem = potionItem.gameObject.AddComponent<PotionStackItem>();
            stackItem.potionItem = potionItem;
            typeof(StackItem).GetField("thisRigidbody", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(stackItem, potionItem.GetRigidbody());
            stackItem.transform.parent = potionItem.transform;
            stackScript.itemsFromThisStack = new List<StackItem> { stackItem };
            var potionSoundController = (PotionCraft.ObjectBased.Stack.SoundController)FormatterServices.GetUninitializedObject(typeof(PotionCraft.ObjectBased.Stack.SoundController));
            InitPotionStackSoundController(potionSoundController, stackScript, ((Potion)potionItem.inventoryItem).soundPreset);
            typeof(ItemFromInventory).GetProperty("SoundController", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(stackScript, potionSoundController);
            stackItem.Initialize(stackScript);
            visualEffect.stackScript = stackScript;
            stackScript.inventoryItem = potionItem.inventoryItem;
            stackItem.spriteRenderers = new SpriteRenderer[0];
            stackItem.graphicStateMachine = potionItem.gameObject.AddComponent<GraphicStateMachine>();
            stackItem.graphicStateMachine.Init(stackItem);
            return;
        }

        private static void InitPotionStackSoundController(PotionCraft.ObjectBased.Stack.SoundController instance, ItemFromInventory itemFromInventory, ItemFromInventoryPreset preset)
        {
            var firstInvIngredient = Managers.Player.inventory.items.Keys.ToList().OfType<Ingredient>().FirstOrDefault();
            if (firstInvIngredient == null)
            {
                Plugin.PluginLogger.LogError("Failed to setup sound controller for potion because there were no ingredients in the player inventory.");
                return;
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

        [HarmonyPatch(typeof(RecipeBook), "OnLoad")]
        public class SetupPotionItemsForPouringInOnRecipeLoadPatch
        {
            static void Postfix()
            {
                Ex.RunSafe(() => SetupPotionItemsForPouringInOnRecipeLoad());
            }
        }

        public static void SetupPotionItemsForPouringInOnRecipeLoad()
        {
            Managers.Game.ItemContainer.GetComponentsInChildren<PotionItem>().Where(p => p.GetComponent<Stack>() == null).ToList().ForEach(SetupPotionItemForPouringIn);
        }

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
        }

        public static bool OverridePotionStackItemSoundController(SoundController instance, StackItem stackItem)
        {
            if (stackItem is not PotionStackItem) return true;
            var potionItem = ((PotionStackItem)stackItem).potionItem;
            typeof(SoundController).GetField("stackItem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, stackItem);
            typeof(SoundController).GetField("newCollisions", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new List<Vector2>());
            typeof(SoundController).GetField("collidedWith", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new List<GameObject>());
            var firstInvIngredient = Managers.Player.inventory.items.Keys.ToList().OfType<Ingredient>().FirstOrDefault();
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
            var potionSoundPreset = ((Potion)potionItem.inventoryItem).soundPreset;
            preset.hit = potionSoundPreset.hit;
            typeof(SoundController).GetField("preset", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, preset);
            typeof(SoundController).GetField("rubLoopEmitter", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, new LoopEmitter(preset.rubLoop));
            return false;
        }

        #endregion

        #region Event Forwarding

        [HarmonyPatch(typeof(ItemFromInventory), "CustomOnReleasePrimaryCondition")]
        public class ForwardPotionItemOnReleasePrimaryPatch //TODO there are probably a lot of mouse events we need to forward like this: OnGamepadButtonBDowned, OnGamepadButtonBUpped, OnGamepadButtonXDowned, OnGamepadButtonXUpped, OnGamepadButtonYDowned, OnGamepadButtonYUpped
        {
            static void Postfix(ItemFromInventory __instance)
            {
                Ex.RunSafe(() => ForwardPotionItemOnReleasePrimary(__instance));
            }
        }

        public static void ForwardPotionItemOnReleasePrimary(ItemFromInventory instance)
        {
            if (instance is not PotionItem) return;
            var stack = instance.gameObject.GetComponent<Stack>();
            if (stack == null) return;
            typeof(Stack).GetMethod("CustomOnReleasePrimaryCondition", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(stack, null);
        }

        [HarmonyPatch(typeof(PotionItem), "OnGrabPrimary")]
        public class ForwardGrabPrimaryPatch
        {
            static void Postfix(PotionItem __instance)
            {
                Ex.RunSafe(() => ForwardGrabPrimary(__instance));
            }
        }
        public static void ForwardGrabPrimary(PotionItem instance)
        {
            var stack = instance.GetComponent<Stack>();
            if (stack == null)
            {
                Plugin.PluginLogger.LogInfo("Grabbed potion does not have recipe data. This is likely a pre-mod potion with no corresponding recipe.");
                return;
            }
            stack.OnGrabPrimary();
        }

        [HarmonyPatch(typeof(ItemFromInventory), "OnReleasePrimary")]
        public class ForwardReleasePrimaryPatch
        {
            static void Postfix(ItemFromInventory __instance)
            {
                Ex.RunSafe(() => ForwardReleasePrimary(__instance));
            }
        }

        public static void ForwardReleasePrimary(ItemFromInventory instance)
        {
            if (instance == null) return;
            if (instance.markedAsDestroyed) return;
            if (instance is not PotionItem potionItem) return;
            var stack = potionItem.GetComponent<Stack>();
            if (stack == null) return;
            stack.OnReleasePrimary();
        }

        #endregion

        #region PotionStackItem Exception Prevention Overrides



        [HarmonyPatch(typeof(StackItem), "OnDestroy")]
        public class PreventStackItemOnDestroyExceptionPatch
        {
            static bool Prefix(StackItem __instance)
            {
                return Ex.RunSafe(() => PreventStackItemOnDestroyException(__instance));
            }
        }

        public static bool PreventStackItemOnDestroyException(StackItem instance)
        {
            if (instance is not PotionStackItem) return true;
            instance.stackScript?.RemoveItemFromStack(instance);
            instance.soundController?.OnDestroy();
            return false;
        }

        [HarmonyPatch(typeof(Stack), "DisassembleStack")]
        public class PreventPotionStackDissassemblyPatch
        {
            static bool Prefix(Stack __instance)
            {
                return Ex.RunSafe(() => PreventPotionStackDissassembly(__instance));
            }
        }

        public static bool PreventPotionStackDissassembly(Stack instance)
        {
            return instance.GetComponent<PotionItem>() == null;
        }

        [HarmonyPatch(typeof(SoundController), "OnGrind")]
        public class OverrideSoundControllerOnGrindPatch
        {
            static bool Prefix(SoundController __instance)
            {
                return Ex.RunSafe(() => OverrideSoundControllerOnGrind(__instance));
            }
        }

        /// <summary>
        /// This should never be called for a PotionStackItem sound controller but it is better to override this method anyways to prevent exceptions should someone manage to grind a potion
        /// </summary>
        public static bool OverrideSoundControllerOnGrind(SoundController instance)
        {
            var stackItem = typeof(SoundController).GetField("stackItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
            return stackItem is not PotionStackItem;
        }

        #endregion
    }
}
