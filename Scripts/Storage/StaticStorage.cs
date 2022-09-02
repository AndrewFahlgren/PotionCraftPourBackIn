using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace PotionCraftPourBackIn.Scripts.Storage
{
    public static class StaticStorage
    {
        public static bool CurrentlyMakingPotion;
        public static bool CurrentlyGeneratingRecipe;
        public static List<Potion.UsedComponent> PouredInUsedComponents;
        public static List<PotionEffect> PouredInEffects;

        public static List<Potion> PotionItemsNotifiedFor = new();
        public static float LastNotifiedTime;
        public static Potion CurrentPotionCraftPanelPotion;

        public static bool DummyEffectAddedToEnableFinishPotionGrab;
    }
}
