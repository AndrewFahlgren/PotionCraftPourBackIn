using PotionCraft.Core.Cursor;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.InteractiveItem;
using PotionCraft.ObjectBased.RecipeMap.Buttons;
using PotionCraft.ObjectBased.UIElements;
using PotionCraft.ObjectBased.UIElements.Tooltip;
using PotionCraftPourBackIn.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PotionCraftPourBackIn.Scripts.UIComponents
{
    public class WaypointToggleButton : InteractiveItem, IPrimaryCursorEventsHandler, ICustomCursorStateOnUse, ICustomCursorStateOnHover, IHoverable
    {
        public SpriteRenderer iconRenderer;
        public BoxCollider2D collider;
        public GameObject spriteSlot;
        public FollowIndicatorButton followButton;
        public RecipeIndex currentRecipe => RecipeService.GetWaypointRecipeAtIndex(Managers.Potion.recipeBook.currentPageIndex);
        public bool IsMapWaypointToggleButton => followButton != null;

        public float OffAlpha => 0.5f;

        public void OnPrimaryCursorClick()
        {
            if (IsMapWaypointToggleButton) 
                UIService.ShowHideWaypoints();
            else 
                RecipeService.ToggleWaypointForSelectedRecipe(currentRecipe);
        }

        public bool OnPrimaryCursorRelease()
        {
            return true;
        }

        public override TooltipContent GetTooltipContent()
        {
            if (IsMapWaypointToggleButton)
            {
                return new TooltipContent
                {
                    header = "Show or Hide Waypoints"
                };
            }
            var recipe = currentRecipe;
            if (recipe == null) return new TooltipContent();
            var hasEffect = recipe.Recipe.Effects.Length > 0 && recipe.Recipe.Effects[0] != null;
            var tooltipText = recipe.IsWaypoint 
                                ? hasEffect 
                                    ? "Unmark this recipe as a waypoint"
                                    : "Hide this waypoint on the map"
                                : hasEffect
                                    ? "Mark this recipe as a waypoint"
                                    : "Show this waypoint on the map";
            return new TooltipContent
            {
                header = tooltipText
            };
        }
        public CursorVisualState CursorStateOnUse() => CursorVisualState.Pressed;
        public CursorVisualState CursorStateOnHover() => CursorVisualState.Pressed;

        public void SetHovered(bool hovered)
        {
            followButton?.SetHovered(hovered);
        }
    }
}
