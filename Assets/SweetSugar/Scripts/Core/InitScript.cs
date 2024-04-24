using System;
using System.Collections.Generic;
using SweetSugar.Scripts.GUI;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.MapScripts;
using SweetSugar.Scripts.System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

namespace SweetSugar.Scripts.Core
{
    public enum LIMIT
    {
        MOVES,
        TIME
    }

    /// reward type for rewarded ads watching
    public enum RewardsType
    {
        GetLifes,
        GetGems,
        GetGoOn
    }
}
