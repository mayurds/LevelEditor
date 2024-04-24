using System.Collections;
using System.Linq;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.System;
using SweetSugar.Scripts.System.Pool;
using UnityEngine;

namespace SweetSugar.Scripts.Items
{
    /// <summary>
    /// Item marmalade
    /// </summary>
    public class ItemMarmalade : Item, IItemInterface
    {
        // public bool Combinable;
        public bool ActivateByExplosion;
        public bool StaticOnStart;
        public bool noMarmaladeLaunch;
        public ItemMarmalade secondItem;
        public MarmaladeFly[] marmalades;
        private bool destroyStarted;

        public void Destroy(Item item1, Item item2)
        {
            if (GetParentItem().square?.type == SquareTypes.WireBlock)
            {
                GetParentItem().square.DestroyBlock();
                return;
            }

            item1.destroying = true;
            var switchItemType = item2?.currentType ?? ItemsTypes.NONE;
           
            if (switchItemType == ItemsTypes.MARMALADE)
                item2.GetTopItemInterface().Destroy(item2, null);
            else if (switchItemType != ItemsTypes.NONE && (item2?.Combinable ?? false))
                item2?.DestroyItem();
            else if( switchItemType == ItemsTypes.MULTICOLOR)
                item2?.DestroyBehaviour();
            else if(noMarmaladeLaunch) DestroyBehaviour();
            GetParentItem().square.DestroyBlock();
        }

 

    

        IEnumerator WaitForReachTarget()
        {
            yield return new WaitWhile(()=>marmalades.Any(i=>i.gameObject.activeSelf));
            DestroyBehaviour();
        }

        private void OnDisable()
        {
            if(square?.Item == this)
                square.Item = null;
        }

        public override void InitItem()
        {
            destroyStarted = false;
            noMarmaladeLaunch = false;
            marmalades.ForEachY(i => i.gameObject.SetActive(true));
            base.InitItem();
        }

        public override void Check(Item item1, Item item2)
        {
            if (item2.currentType == ItemsTypes.MULTICOLOR)
            {
                item2.Check(item2, item1);
            }
            else if (item2.currentType != ItemsTypes.NONE)
                Destroy(item1, item2);
            LevelManager.THIS.FindMatches();

        }

        public Item GetParentItem()
        {
            return GetComponent<Item>();
        }

        public GameObject GetGameobject()
        {
            return gameObject;
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

        public void SetOrder(int i)
        {
            GetComponent<SpriteRenderer>().sortingOrder = i;
        }

    }
}
