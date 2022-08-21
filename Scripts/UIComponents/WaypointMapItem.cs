using PotionCraft.Core.Cursor;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.UIElements.Tooltip;
using PotionCraftPourBackIn.Scripts.Services;
using UnityEngine;

namespace PotionCraftPourBackIn.Scripts.UIComponents
{
    public class WaypointMapItem : PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.RecipeMapItem, IPrimaryCursorEventsHandler, ICustomCursorStateOnUse, ICustomCursorStateOnHover, IHoverable
    {
        public const float WaypointAlpha = 0.75f;

        public RecipeIndex Recipe;
        public SpriteRenderer IconRenderer;
        public CircleCollider2D circleCollider;
        public GameObject path;
        private bool loadedPath;

        public void OnPrimaryCursorClick()
        {
            Managers.Potion.recipeBook.OpenPageAt(Recipe.Index);
        }

        public bool OnPrimaryCursorRelease()
        {
            return true;
        }

        public override TooltipContent GetTooltipContent()
        {
            return Recipe.Recipe.GetTooltipContent(1);
        }

        public CursorVisualState CursorStateOnUse() => CursorVisualState.Pressed;
        public CursorVisualState CursorStateOnHover() => CursorVisualState.Pressed;

        public void SetHovered(bool hovered)
        {
            if (hovered && !loadedPath)
            {
                UIService.CreateWaypointHoverPath(this);
                loadedPath = true;
            }
            path.SetActive(hovered);
        }
    }
}
