using SweetSugar.Scripts.Core;
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
	

		public void Settings () {


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
