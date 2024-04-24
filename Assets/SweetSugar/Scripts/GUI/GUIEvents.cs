using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.Integrations;
using SweetSugar.Scripts.System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SweetSugar.Scripts.GUI
{
	/// <summary>
	/// GUI events for Facebook, Settings and main scene
	/// </summary>
	public class GUIEvents : MonoBehaviour {
		public GameObject loading;
		void Update () {
			if (name == "FaceBook" || name == "Share" || name == "FaceBookLogout") {
				if (!LevelManager.THIS.FacebookEnable)
					gameObject.SetActive (false);
			}
		}

		public void Settings () {


		}

		public void Play () {
			LeanTween.delayedCall(1, ()=>SceneManager.LoadScene("game"));
		}

		public void Pause () {

			if (LevelManager.THIS.gameStatus == GameState.Playing)
				GameObject.Find ("CanvasGlobal").transform.Find ("MenuPause").gameObject.SetActive (true);

		}

		public void FaceBookLogin () {
#if FACEBOOK

			FacebookManager.THIS.CallFBLogin ();
#endif
		}

		public void FaceBookLogout () {
#if FACEBOOK
			FacebookManager.THIS.CallFBLogout ();

#endif
		}

		public void Share () {
#if FACEBOOK

			FacebookManager.THIS.Share ();
#endif
		}

	}
}
