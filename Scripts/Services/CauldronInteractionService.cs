using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.Cauldron;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.InteractiveItem.Vacuuming;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.Stack.StackItem;
using PotionCraft.ObjectOptimizationSystem;
using PotionCraft.ScriptableObjects;
using PotionCraft.SoundSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using SoundController = PotionCraft.ObjectBased.Stack.StackItem.SoundController;

namespace PotionCraftPourBackIn.Scripts.Services
{
    /// <summary>
    /// Responsible for cauldron interactions like drop detection and animations
    /// </summary>
    public static class CauldronInteractionService
    {
        //TODO we also need to vaccum up the potion if possible and change the cauldron outline to look like when an ingredient is going to get vaccumed
        public static bool AcceptPotionCollision(Collider2D other)
        {
            var potionItem = other.GetComponentInParent<PotionItem>();
            if (potionItem == null) return true;

            if (Managers.Potion.potionCraftPanel.IsPotionBrewingStarted()) return true;

            var potion = potionItem.inventoryItem as Potion;

            //TODO this might be being called multiple times. At least the splash is triggering multiple times
            PotionService.ContinueBrewingFromPotion(potion);
            AnimateSplash(potion, potionItem);
            MovePotionBehindCauldron(potion, potionItem);

            return false;
        }

        //TODO the potion is resting on the bottom of the cauldron. is this the recycle mod?
        private static void AnimateSplash(Potion potion, PotionItem potionItem)
        {
            var position = (Vector2)potionItem.transform.position;
            var cauldron = Managers.Ingredient.cauldron;
            
            var cauldronFxAnimator = cauldron.cauldronFxAnimator;
            var soundController = cauldron.soundController;
            soundController.PlayDropItem();
            var potionRigidBody = typeof(MovableItem).GetField("thisRigidbody", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(potionItem) as Rigidbody2D;
            CauldronFxAnimator.PlaySplash(CauldronFxAnimator.SplashType.Large, position, potionRigidBody.velocity);

        }

        private static void MovePotionBehindCauldron(Potion potion, PotionItem potionItem)
        {
           // potionItem.sortingOrderSetter.rend
        }

        private static void DeletePotion(Potion potion)
        {
            //here we need to remove the potion from the game world
        }


        //TODO we may need to disable rigid body simulation when we drop the potion in the cauldron. If so do this on update:
        //ItemFromInventoryPhysicsOptimizer.DisableRigidbodySimulation((IPhysicsOptimizerTarget) this);


        public static bool AllowCauldronCollision(PotionItem instance, Collider2D target, bool ignore)
        {
            if (!ignore) return true;
            Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 1");
            var cauldronVaccum = Managers.Ingredient.cauldron?.throwVacuuming;
            Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 2");
            if (cauldronVaccum == null) return true;
            Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 3");
            var cauldronCollider = typeof(ThrowVacuuming).GetField("colliderPhysics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cauldronVaccum) as Collider2D;
            Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 4");
            if (target == cauldronCollider)
            {
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 5");
                var visualEffect = instance.gameObject.AddComponent<StackVisualEffects>();
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 6");
                var stackItem = instance.gameObject.AddComponent<PotionStackItem>();
                stackItem.potionItem = instance;
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 7");
                stackItem.transform.parent = instance.transform;
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 8");
                var stackScript = instance.gameObject.AddComponent<Stack>();
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 9");
                stackScript.itemsFromThisStack = new List<StackItem> { stackItem };
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 10");
                stackItem.Initialize(stackScript); //TODO null reference here
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 11");
                visualEffect.stackScript = stackScript;
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 12");
                //TODO copy all stuff from potionitem interactiveitem

                stackScript.inventoryItem = instance.inventoryItem;
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 13");

                //internal Vector2 positionInStack;
                //typeof(StackItem).GetField("positionInStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(stackItem, )
                //internal float rotationInStack;

                //internal Rigidbody2D thisRigidbody;

                typeof(StackItem).GetField("thisRigidbody", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(stackItem, instance.GetRigidbody());
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 14");

                //public SpriteRenderer[] spriteRenderers;
                stackItem.spriteRenderers = new SpriteRenderer[] { instance.visualObject.bottleLiquidMainSpriteRenderer };
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 15");

                stackItem.graphicStateMachine = instance.gameObject.AddComponent<GraphicStateMachine>();
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 16");
                stackItem.graphicStateMachine.Init(stackItem);
                Plugin.PluginLogger.LogInfo("AllowCauldronCollision - 17");


                return false;
            }
            return true;
        }

        public static bool OverridePotionStackItemSoundController(SoundController instance, StackItem stackItem)
        {
            //TODO recreate method without crashing
            //instance.stackItem = stackItem;
            //instance.preset = ((PotionCraft.ScriptableObjects.Ingredient.Ingredient)stackItem.stackScript.inventoryItem).soundPreset;
            //instance.rubLoopEmitter = new LoopEmitter(instance.preset.rubLoop);
            return true;
        }
    }
}
