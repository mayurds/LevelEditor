using System;
using System.Collections.Generic;
using System.Linq;
using SweetSugar.Scripts.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SweetSugar.Scripts.System.Pool
{

    /// <summary>
    /// object pool. Uses to keep and activate items and effects
    /// </summary>
    [Serializable]
    public class ObjectPoolItem
    {
        public GameObject objectToPool;
        public string poolName;
        public int amountToPool;
        public bool shouldExpand = true;
        public bool inEditor = true;
    }

    [ExecuteInEditMode]
    public class ObjectPooler : MonoBehaviour
    {
        public const string DefaultRootObjectPoolName = "Pooled Objects";

        public static ObjectPooler Instance;
        public string rootPoolName = DefaultRootObjectPoolName;
        public List<PoolBehaviour> pooledObjects = new List<PoolBehaviour>();
        private List<ObjectPoolItem> itemsToPool;


        void OnEnable()
        {
            LoadFromScriptable();
            Instance = this;
        }

        private void LoadFromScriptable()
        {
        }

        private void Start()
        {
            if (!Application.isPlaying) return;
            ClearNullElements();

            foreach (var item in itemsToPool)
            {
                if (item == null) continue;
                if (item.objectToPool == null) continue;
                var pooledCount = pooledObjects.Count(i => i.name == item.objectToPool.name);
                for (int i = 0; i < item.amountToPool - pooledCount; i++)
                {
                    CreatePooledObject(item);
                }
            }
        }

        private void ClearNullElements()
        {
            pooledObjects.RemoveAll(i => i == null);
        }

        private GameObject GetParentPoolObject(string objectPoolName)
        {
            // Use the root object pool name if no name was specified
            // if (string.IsNullOrEmpty(objectPoolName))
            //     objectPoolName = rootPoolName;

            // if (GameObject.Find(rootPoolName) == null) new GameObject { name = rootPoolName };
            GameObject parentObject = GameObject.Find(objectPoolName);
            // Create the parent object if necessary
            if (parentObject == null)
            {
                parentObject = new GameObject();
                parentObject.name = objectPoolName;

                // Add sub pools to the root object pool if necessary
                if (objectPoolName != rootPoolName)
                    parentObject.transform.parent = transform;
            }

            return parentObject;
        }

        public void HideObjects(string tag)
        {
            // Debug.Log("hide");
            var objects = GameObject.FindObjectsOfType<PoolBehaviour>().Where(i=>i.name == tag);
            foreach (var item in objects)
                item.gameObject.SetActive(false);
        }

      

        private PoolBehaviour CreatePooledObject(ObjectPoolItem item)
        {
            // if (!Application.isPlaying && !item.inEditor)
            // {
            //     Debug.Log("not play not editor - " + item.objectToPool);
            //     return null;
            // }
            GameObject obj = Instantiate(item.objectToPool);
            // Get the parent for this pooled object and assign the new object to it
            var parentPoolObject = GetParentPoolObject(item.poolName);
            obj.transform.parent = parentPoolObject.transform;
            var poolBehaviour = obj.AddComponent<PoolBehaviour>();
            poolBehaviour.name = item.objectToPool.name;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                obj = PrefabUtility.ConnectGameObjectToPrefab(obj, item.objectToPool);
            }
#endif

            obj.SetActive(false);
            pooledObjects.Add(poolBehaviour);


            return poolBehaviour;
        }

        public void DestroyObjects(string tag)
        {
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (pooledObjects[i].name == tag)
                {
                    DestroyImmediate(pooledObjects[i]);
                }
            }
            ClearNullElements();
        }
    }
}