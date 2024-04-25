using System.Linq;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSugar.Scripts.GUI
{
    /// <summary>
    /// Target icon in the MenuPlay
    /// </summary>
    public class MenuTargetIcon : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI description;
        private Image[] images;

        private void Awake()
        {
            images = transform.GetChildren().Select(i => i.GetComponent<Image>()).ToArray();
        }

    void OnEnable()
    {
        DisableImages();
        var levelData = new LevelData(Application.isPlaying, LevelManager.THIS.currentLevel);
        levelData = LoadingManager.LoadForPlay(PlayerPrefs.GetInt("OpenLevel"), levelData);
    }

        private void DisableImages()
        {
            foreach (var item in images)
            {
                item.gameObject.SetActive(false);
            }
        }

        void OnDisable()
        {
            // for (var i = transform.childCount - 1; i >= 1; i--)
            // {
            //     Destroy(transform.GetChild(i).gameObject);
            // }
        }
    }
}
