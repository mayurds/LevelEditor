using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SweetSugar.Scripts.Core;
using UnityEngine;

namespace SweetSugar.Scripts.Items
{
    /// <summary>
    /// Destroy processing. Delayed destroying items from array
    /// </summary>
    public class DestroyingPipeline : MonoBehaviour
    {
        public static DestroyingPipeline THIS;
        public List<DestroyBunch> pipeline = new List<DestroyBunch>();

        private void Start()
        {
            if (THIS == null)
                THIS = this;
            else if (THIS != this)
                Destroy(gameObject);

        }

        public void DestroyItems(List<Item> items, Delays delays, Action callback)
        {
           
            var bunch = new DestroyBunch();
            bunch.items = items.ToList();
            bunch.callback = callback;
            bunch.delays = delays;
            pipeline.Add(bunch);
        }

    }

    public class DestroyBunch
    {
        public List<Item> items;
        public Action callback;
        public Delays delays;
    }

    public struct Delays
    {
        public CustomYieldInstruction before, beforeevery, afterevery, after;

    }
}