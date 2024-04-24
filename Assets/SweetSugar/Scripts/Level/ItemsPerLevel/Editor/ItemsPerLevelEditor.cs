using UnityEditor;
using UnityEngine;

namespace SweetSugar.Scripts.Level.ItemsPerLevel.Editor
{
    public class ItemsPerLevelEditor : EditorWindow
    {
        private static GameObject prefab;
        private static int numLevel;

        // Add menu item named "My Window" to the Window menu
        public static void ShowWindow(GameObject itemPrefab, int level)
        {
            ItemsPerLevelEditor window = (ItemsPerLevelEditor) EditorWindow.GetWindow(typeof(ItemsPerLevelEditor), true, itemPrefab.name + " editor");

            prefab = Resources.Load<GameObject>("Items/" + itemPrefab.name);
            numLevel = level;
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow(typeof(ItemsPerLevelEditor));
        }


    }
}