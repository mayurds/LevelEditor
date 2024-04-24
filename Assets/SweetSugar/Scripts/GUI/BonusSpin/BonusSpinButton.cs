using System;
using SweetSugar.Scripts.System;
using UnityEngine;

namespace SweetSugar.Scripts.GUI.BonusSpin
{
    /// <summary>
    /// Opens spinning wheel bonus game
    /// </summary>
    public class BonusSpinButton : MonoBehaviour {
        public GameObject spin;

   
        /// <summary>
        /// Check server to show or hide the button
        /// </summary>
    
        private void OnDisable()
        {
//        ServerTime.OnDateReceived -= CheckSpin;
        }

        public void OnClick()
        {
            spin.SetActive(true);
        }

        public void OnSpin()
        {
            SetDate();
        }

        void SetDate(){
        }
    }
}