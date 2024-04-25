using SweetSugar.Scripts.Core;
using TMPro;
using UnityEngine;

namespace SweetSugar.Scripts.GUI
{
    /// <summary>
    /// Moves / Time label in the game
    /// </summary>
    public class MovesLabel : MonoBehaviour
    {
        // Use this for initialization
        void OnEnable()
        {
            if(LevelManager.THIS?.levelData == null || !LevelManager.THIS.levelLoaded)
                LevelManager.OnLevelLoaded += Reset;
            else 
                Reset();
        }

        void OnDisable()
        {
            LevelManager.OnLevelLoaded -= Reset;
        }


    void Reset()
    {
        if (LevelManager.THIS != null && LevelManager.THIS.levelLoaded)
        {
        }

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
