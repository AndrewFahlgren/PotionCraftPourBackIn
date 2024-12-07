using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ScriptableObjects.Potion;
using System.Collections.Generic;
using System.Linq;
using static PotionCraft.SaveLoadSystem.ProgressState;

namespace PotionCraftPourBackIn.Scripts.Services
{
    /// <summary>
    /// Service responsible for interacting with the Potion Craft recipe systems
    /// </summary>
    public static class RecipeService
    {
        public static SerializedPotionRecipeData GetRecipeForPotion(Potion potion)
        {
            return RecipeBook.Instance.savedRecipes.FirstOrDefault(recipe => RecipeMatchesPotion(potion, recipe))?.GetSerializedRecipeData() as SerializedPotionRecipeData;
        }

        private static bool RecipeMatchesPotion(Potion potion, IRecipeBookPageContent recipe)
        {
            if (recipe == null) return false;
            if (!recipe.GetLocalizedTitle().Equals(potion.GetLocalizedTitle())) return false;
            if (recipe.Effects.Count() != potion.Effects.Count()) return false;
            if (!SequencesMatch(recipe.Effects.Select(e => e.name).ToList(), potion.Effects.Select(e => e.name).ToList())) return false;
            if (recipe.usedComponents.Count() != potion.usedComponents.Count()) return false;
            var recipeUsedComps = recipe.usedComponents.Select(e => new { e.componentObject.name, e.amount }).ToList();
            var potionUsedComps = potion.usedComponents.Select(e => new { e.componentObject.name, e.amount }).ToList();
            if (!SequencesMatch(recipeUsedComps, potionUsedComps)) return false;
            return true;
        }

        private static bool SequencesMatch<T>(IList<T> seq1, IList<T> seq2)
        {
            if (seq1.Count != seq2.Count) return false;
            for (var i = 0; i < seq1.Count; i++)
            {
                if (!seq1[i].Equals(seq2[i])) return false;
            }
            return true;
        }
    }
}
