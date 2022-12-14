using PotionCraft.ScriptableObjects;
using PotionCraft.ScriptableObjects.Potion;
using System.Collections.Generic;

namespace PotionCraftPourBackIn.Scripts.Storage
{
    public static class StaticStorage
    {
        public static bool CurrentlyMakingPotion;
        public static bool CurrentlyGeneratingRecipe;
        public static List<PotionUsedComponent> PouredInUsedComponents;
        public static List<PotionEffect> PouredInEffects;

        public static List<Potion> PotionItemsNotifiedFor = new();
        public static float LastNotifiedTime;
        public static Potion CurrentPotionCraftPanelPotion;

        public static bool DummyEffectAddedToEnableFinishPotionGrab;
    }
}
