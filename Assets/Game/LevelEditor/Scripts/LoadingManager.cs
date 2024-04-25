using System.IO;
using UnityEngine;

    public static class LoadingManager
    {
        public static LevelData LoadForPlay(int currentLevel)
        {
            LevelData levelData = new LevelData(currentLevel);
            levelData = LoadlLevel(currentLevel, levelData).DeepCopyForPlay(currentLevel);
            return levelData;
        }
        public static LevelData LoadlLevel(int currentLevel, LevelData levelData)
        {
            levelData = ScriptableLevelManager.LoadLevel(currentLevel);
            LevelData.THIS = levelData;
            return levelData;
        }
        public static int GetLastLevelNum()
        {
            return Resources.LoadAll<LevelContainer>("Levels").Length;
        }
    }

