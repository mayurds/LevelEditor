﻿using System.IO;
using SweetSugar.Scripts.System;
using UnityEngine;

namespace SweetSugar.Scripts.Level
{
    /// <summary>
    /// Loading level 
    /// </summary>
    public static class LoadingManager
    {
        private static LevelData levelData;
        static string levelPath = "Assets/Game/Resources/Levels/";

        public static LevelData LoadForPlay(int currentLevel, LevelData levelData)
        {
            levelData = new LevelData(Application.isPlaying, currentLevel);
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
}
