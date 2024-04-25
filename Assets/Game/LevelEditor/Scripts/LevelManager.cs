using System.Collections.Generic;
using UnityEngine;


    public class LevelManager : MonoBehaviour
    {
        public static LevelManager THIS;
        public int currentLevel = 1;
        private void OnEnable()
        {
            levelData = LoadingManager.LoadForPlay(currentLevel, levelData);
           
        }
        private void Awake()
        {
            THIS = this;
        }
        public LevelData levelData;

    }
