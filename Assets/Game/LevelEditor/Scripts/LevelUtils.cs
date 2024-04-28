using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelUtils 
{
    public static LevelData LoadForPlay(int currentLevel)
    {
        LevelData levelData = new LevelData(currentLevel);
        levelData = LoadlLevel(currentLevel, levelData).DeepCopyForPlay(currentLevel);
        return levelData;
    }
    public static LevelData LoadlLevel(int currentLevel, LevelData levelData)
    {
        levelData = LoadLevel(currentLevel);
        LevelData.THIS = levelData;
        return levelData;
    }
    public static int GetLastLevelNum()
    {
        return Resources.LoadAll<LevelContainer>("Levels").Length;
    }

#if UNITY_EDITOR
    public static void CreateFileLevel(int level, LevelData _levelData)
    {
        var path = "Assets/SweetSugar/Resources/Levels/";

        if (Resources.Load("Levels/Level_" + level))
        {
            SaveLevel(path, level, _levelData);
        }
        else
        {
            string fileName = "Level_" + level;
            var newLevelData = CreateAsset<LevelContainer>(path, fileName);
            newLevelData.SetData(_levelData.DeepCopy(level));
            EditorUtility.SetDirty(newLevelData);
            AssetDatabase.SaveAssets();
        }
    }
    public static void SaveLevel(string path, int level, LevelData _levelData)
    {
        var levelScriptable = Resources.Load("Levels/Level_" + level) as LevelContainer;
        if (levelScriptable != null)
        {
            levelScriptable.SetData(_levelData.DeepCopy(level));
            EditorUtility.SetDirty(levelScriptable);
        }

        AssetDatabase.SaveAssets();
    }

    public static T CreateAsset<T>(string path, string name) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + name + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        return asset;
    }
#endif

    public static LevelData LoadLevel(int level)
    {
        var levelScriptable = Resources.Load("Levels/Level_" + level) as LevelContainer;
        LevelData levelData;
        if (levelScriptable)
        {
            levelData = levelScriptable.levelData.DeepCopy(level);
        }
        else
        {
            var levelScriptables = Resources.Load("Levels/LevelScriptable") as LevelScriptable;
            var ld = levelScriptables.levels.TryGetElement(level - 1, null);
            levelData = ld.DeepCopy(level);
        }

        return levelData;
    }
}
