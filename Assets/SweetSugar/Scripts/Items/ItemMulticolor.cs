﻿using System;
using System.Collections;
using System.Linq;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SweetSugar.Scripts.Items
{
    /// <summary>
    /// Item multicolor
    /// </summary>
    public class ItemMulticolor : Item, IItemInterface
    {
        // public bool Combinable;
        public bool ActivateByExplosion;
        public bool StaticOnStart;

        public GameObject LightningPrefab;
        private bool jellySpread;

        public override void Check(Item item1, Item item2)
        {
            if (item1?.square?.type == SquareTypes.JellyBlock || item2?.square?.type == SquareTypes.JellyBlock)
                jellySpread = true;
            if (item2 != null && ((!item2.Combinable && item2.currentType != ItemsTypes.MULTICOLOR)))
                return;
            GetParentItem().destroying = true;
            if (item2.currentType == ItemsTypes.NONE)
            {
                item2.DestroyItem();
            }
            else if (item2.currentType == ItemsTypes.HORIZONTAL_STRIPED || item2.currentType == ItemsTypes.VERTICAL_STRIPED)
                LevelManager.THIS.StartCoroutine(SetTypeByColor(item2));
            else if (item2.currentType == ItemsTypes.PACKAGE)
                LevelManager.THIS.StartCoroutine(SetTypeByColor(item2));
            else if (item2.currentType == ItemsTypes.MARMALADE)
                LevelManager.THIS.StartCoroutine(SetTypeByColor(item2));
            else if (item2.currentType == ItemsTypes.MULTICOLOR)
            {
                item2.SmoothDestroy();
                DestroyDoubleMulticolor(item1.square.col, () =>
                {
                    var list = new[] { item1, item2 };
                    list.First(i => i != GetParentItem()).SmoothDestroy();
                    list.First(i => i == GetParentItem()).SmoothDestroy();
//                LevelManager.THIS.DragBlocked = false;
                });
//            item1.HideSprites(true);
//            item2.HideSprites(true);
            }
        }

        public void Destroy(Item item1, Item item2)
        {
            if (item2 == null)
            {
                if (GetParentItem().square.type == SquareTypes.WireBlock)
                {
                    GetParentItem().square.DestroyBlock();
                }

                if (LevelManager.GetGameStatus() == GameState.PreWinAnimations)
                {
                    item2 = LevelManager.THIS.field.squaresArray.First(i => i.item != null && i.item.currentType == ItemsTypes.NONE).item;
                    Check(item1, item2);
                }
                // GetParentItem().StopDestroy();

                return;
            }
            item1?.SmoothDestroy();
        }



        #region ChangeItemTypes

        private IEnumerator SetTypeByColor(Item item2)
        {
            //		SetTypeByColor(item2.color, item2.currentType);
            var nextType = item2.currentType;
            bool loopFinished = false;
            GameObject itemMarmaladeTarget = null;
            itemMarmaladeTarget = new GameObject();
            item2.DestroyItem();
          
            yield return new WaitUntil(() => loopFinished);
                SmoothDestroy();
            //		var list = LevelManager.This.field.GetItems().Where(i => i.currentType == nextType).ToList();

            //		yield return new WaitWhileDestroyPipeline(list, new Delays() { afterevery = new WaitForSecCustom() { s = 0.1f } });
        }

        #endregion

        private IEnumerator IterateItems(Item[] items, Action<Item> iterateItem, Action onFinished = null)
        {
            var i = 0;
            foreach (var item in items)
            {
                if (item != null && item.gameObject.activeSelf)
                {
                    i++;
                    if (jellySpread)
                        item.square?.SetType(SquareTypes.JellyBlock, 1, SquareTypes.NONE, 1);
                    iterateItem(item); 
                    if (i % (Mathf.Clamp(items.Length,items.Length,2)) == 0)
                        yield return new WaitForSeconds(0.2f);
                }
            }
            if (onFinished != null)
                yield return new WaitForSeconds(0.2f);
            onFinished();

        }


        #region DoubleMulitcolor
        public void DestroyDoubleMulticolor(int col, Action callback)
        {
            LevelManager.THIS.field.GetItems();
        }



        #endregion

        public Item GetParentItem()
        {
            return transform.GetComponentInParent<Item>();
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
