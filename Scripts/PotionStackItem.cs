using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ObjectBased.Stack.StackItem;
using PotionCraft.Utils.SortingOrderSetter;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PotionCraftPourBackIn.Scripts
{
    public class PotionStackItem : StackItem
    {
        public PotionItem potionItem;

        //TODO move more logic into this class

        public override Bounds? GetHoveringColliderBounds()
        {
            return potionItem.mainCollider.bounds;
        }

       
        public override void SetPhysicsToDefault()
        {
            potionItem.gameObject.layer = 0;
        }

        public override void SetThisItemLayersToNormal()
        {
            potionItem.gameObject.layer = 14;
        }
    }
}
