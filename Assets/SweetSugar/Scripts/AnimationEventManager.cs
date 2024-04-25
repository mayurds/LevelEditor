using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_ADS
using UnityEngine.Advertisements;
#endif





namespace SweetSugar.Scripts.GUI
{
    /// <summary>
    /// Popups animation event manager
    /// </summary>
    public class AnimationEventManager : MonoBehaviour
    {
        public bool PlayOnEnable = true;
        bool WaitForPickupFriends;

        bool WaitForAksFriends;
        Dictionary<string, string> parameters;

        void OnEnable()
        {
            if (PlayOnEnable)
            {
                //            SoundBase.Instance.PlayOneShot(SoundBase.Instance.swish[0]);

            }
            if (name == "MenuPlay")
            {

            }

            if (name == "PrePlay")
            {
                // GameObject
            }
            if (name == "PreFailed")
            {
//            SoundBase.Instance.PlayOneShot(SoundBase.Instance.gameOver[0]);
                transform.Find("Banner/Buttons/Video").gameObject.SetActive(false);
                transform.Find("Banner/Buttons/Buy").GetComponent<Button>().interactable = true;

                GetComponent<Animation>().Play();
            }

            if (name == "Settings" || name == "MenuPause")
            {
                if (PlayerPrefs.GetInt("Sound") < 1)
                {
                    transform.Find("Sound/Sound/SoundOff").gameObject.SetActive(true);
                    transform.Find("Sound/Sound").GetComponent<Image>().enabled = false;
                }
                else
                {
                    transform.Find("Sound/Sound/SoundOff").gameObject.SetActive(false);
                    transform.Find("Sound/Sound").GetComponent<Image>().enabled = true;
                }

                if (PlayerPrefs.GetInt("Music") < 1)
                {
                    transform.Find("Music/Music/MusicOff").gameObject.SetActive(true);
                    transform.Find("Music/Music").GetComponent<Image>().enabled = false;
                }
                else
                {
                    transform.Find("Music/Music/MusicOff").gameObject.SetActive(false);
                    transform.Find("Music/Music").GetComponent<Image>().enabled = true;
                }

            }

            if (name == "GemsShop")
            {
            }
            if (name == "MenuComplete")
            {
                for (var i = 1; i <= 3; i++)
                {
                    transform.Find("Image").Find("Star" + i).gameObject.SetActive(false);
                }

            }
            if (transform.Find("Image/Video") != null)
            {
#if UNITY_ADS
            AdsManager.THIS.rewardedVideoZone = "rewardedVideo";

			if (!AdsManager.THIS.enableUnityAds || !AdsManager.THIS.GetRewardedUnityAdsReady ())
				transform.Find ("Image/Video").gameObject.SetActive (false);
#else
                transform.Find("Image/Video").gameObject.SetActive(false);
#endif
            }

        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (name == "MenuPlay" || name == "Settings" || name == "BoostInfo" || name == "GemsShop" || name == "LiveShop" || name == "BoostShop" || name == "Reward")
                    CloseMenu();
            }
        }


        /// <summary>
        /// Open rate store
        /// </summary>
 
        void OnDisable()
        {
            if (transform.Find("Image/Video") != null)
            {
                transform.Find("Image/Video").gameObject.SetActive(true);
            }

            //if( PlayOnEnable )
            //{
            //    if( !GetComponent<SequencePlayer>().sequenceArray[0].isPlaying )
            //        GetComponent<SequencePlayer>().sequenceArray[0].Play
            //}
        }

        /// <summary>
        /// Event on finish animation
        /// </summary>
        public void OnFinished()
        {
            if (name == "MenuPause")
            {
                if (LevelManager.THIS.gameStatus == GameState.Playing)
                    LevelManager.THIS.gameStatus = GameState.Pause;
            }

            if (name == "PrePlay")
            {
                CloseMenu();
                LevelManager.THIS.gameStatus = GameState.Tutorial;

            }
            if (name == "PreFailed")
            {
                transform.Find("Banner/Buttons/Video").gameObject.SetActive(false);
                CloseMenu();
            }

            if (name.Contains("gratzWord"))
                gameObject.SetActive(false);
            if (name == "NoMoreMatches")
                gameObject.SetActive(false);
            // if (name == "CompleteLabel")
            //     gameObject.SetActive(false);

        }
        public void WaitForGiveUp()
        {
            if (name == "PreFailed" && LevelManager.THIS.gameStatus != GameState.Playing)
            {
                GetComponent<Animation>()["bannerFailed"].speed = 0;
#if UNITY_ADS

			if (AdsManager.THIS.enableUnityAds) {

				if (AdsManager.THIS.GetRewardedUnityAdsReady ()) {
					transform.Find ("Banner/Buttons/Video").gameObject.SetActive (true);
				}
			}
#endif
            }
        }

        /// <summary>
        /// Complete popup animation
        /// </summary>
        public void Info()
        {
            OpneMenu(gameObject);
        }


        public void OpneMenu(GameObject menu)
        {
            if (menu.activeSelf)
                menu.SetActive(false);
            else
                menu.SetActive(true);
        }

        public IEnumerator Close()
        {
            yield return new WaitForSeconds(0.5f);
        }

        public void CloseMenu()
        {
            if (gameObject.name == "MenuPreGameOver")
            {
                ShowGameOver();
            }
            if (gameObject.name == "MenuComplete")
            {
//            LevelManager.THIS.gameStatus = GameState.Map;
                PlayerPrefs.SetInt("OpenLevel", LevelManager.THIS.currentLevel + 1);
                CrosssceneData.openNextLevel = true;
                SceneManager.LoadScene("game");
            }
            if (gameObject.name == "MenuFailed")
            {
                LevelManager.THIS.gameStatus = GameState.Map;
            }

            if (SceneManager.GetActiveScene().name == "game")
            {
                if (LevelManager.THIS.gameStatus == GameState.Pause)
                {
                    LevelManager.THIS.gameStatus = GameState.WaitAfterClose;

                }
            }

            if (gameObject.name == "Settings" && LevelManager.GetGameStatus() != GameState.Map)
            {
                BackToMap();
            }
            else if (gameObject.name == "Settings" && LevelManager.GetGameStatus() == GameState.Map)
                SceneManager.LoadScene("main");

            //        SoundBase.Instance.PlayOneShot(SoundBase.Instance.swish[1]);

            gameObject.SetActive(false);
        }

        public void SwishSound()
        {

        }

        public void ShowInfo()
        {
            GameObject.Find("CanvasGlobal").transform.Find("BoostInfo").gameObject.SetActive(true);

        }


        public void BackToMap()
        {
            Time.timeScale = 1;
            // LevelManager.THIS.gameStatus = GameState.GameOver;
            // CloseMenu();
            gameObject.SetActive(false);
            LevelManager.THIS.gameStatus = GameState.Map;
            SceneManager.LoadScene("game");
        }

        public void Next()
        {

            CloseMenu();
        }

        [UsedImplicitly]
        public void Again()
        {
            GameObject gm = new GameObject();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }

        public void GoOnFailed()
        {
        }

        [UsedImplicitly]
        public void GiveUp()
        {
        }

        void ShowGameOver()
        {
            GameObject.Find("Canvas").transform.Find("MenuGameOver").gameObject.SetActive(true);
            gameObject.SetActive(false);

        }

        #region boosts


        #endregion

        public void SoundOff(GameObject Off)
        {
            var on = Off.transform.parent;

            if (!Off.activeSelf)
            {
                //            SoundBase.Instance.volume = 0;
                on.GetComponent<Image>().enabled = false;
                Off.SetActive(true);
            }
            else
            {
                //            SoundBase.Instance.volume = 1;

                Off.SetActive(false);
                on.GetComponent<Image>().enabled = true;

            }

            float vol;
            PlayerPrefs.Save();

        }

        public void MusicOff(GameObject Off)
        {
            var on = Off.transform.parent;
            if (!Off.activeSelf)
            {
                //            GameObject.Find("Music").GetComponent<AudioSource>().volume = 0;
                on.GetComponent<Image>().enabled = false;

                Off.SetActive(true);
            }
            else
            {
                //            GameObject.Find("Music").GetComponent<AudioSource>().volume = 1;

                Off.SetActive(false);
                on.GetComponent<Image>().enabled = true;

            }
            float vol;
            PlayerPrefs.Save();

        }

    }
}
