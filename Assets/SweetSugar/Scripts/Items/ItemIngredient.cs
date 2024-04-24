using System;
using SweetSugar.Scripts.Core;
using UnityEngine;

namespace SweetSugar.Scripts.Items
{
    /// <summary>
    /// Item ingredient
    /// </summary>
    public class ItemIngredient : Item, IItemInterface
    {
        public bool ActivateByExplosion;
        public bool StaticOnStart;

        public override void Check(Item item1, Item item2)
        {

        }

        public void Destroy(Item item1, Item item2)
        {
            DestroyBehaviour();
        }

        public GameObject GetGameobject()
        {
            return gameObject;
        }

        public Item GetParentItem()
        {
            return transform.GetComponentInParent<Item>();
        }

        public bool IsCombinable()
        {
            return Combinable;
        }

        public bool IsExplodable()
        {
            return ActivateByExplosion;
        }

        public void SetExplodable(bool setExplodable)
        {
            ActivateByExplosion = setExplodable;
        }

        public bool IsStaticOnStart()
        {
            return StaticOnStart;
        }

        public void SecondPartDestroyAnimation(Action callback)
        {
            throw new NotImplementedException();
        }

        public void SetOrder(int i)
        {
            GetComponent<SpriteRenderer>().sortingOrder = i;
        }



    }
}
