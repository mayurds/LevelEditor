using System;
using SweetSugar.Scripts.Blocks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SweetSugar.Scripts.TargetScripts.TargetSystem
{
    /// <summary>
    /// target container keeps the object should be collected, its count, sprite, color
    /// </summary>
    [Serializable]
    public class SubTargetContainer
    {
        ///using to keep count of targets
        public int count;
        public int preCount;
        public int color;
        public SubTargetContainer( int _count)
        {
            count = _count;
            preCount = count;
        }

        public void changeCount(int i)
        {
            count += i;
            if (count < 0) count = 0;
            preCount = count;
        }

        public int GetCount()
        {
            return count;
        }

        public void SetCount(int i)
        {
            count = i;
        }

      
        public SubTargetContainer DeepCopy()
        {
            var other = new SubTargetContainer( count);
            return other;
        }
    }
}