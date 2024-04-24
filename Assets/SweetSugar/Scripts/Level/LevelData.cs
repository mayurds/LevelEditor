using System;
using System.Collections.Generic;
using System.Linq;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.Items;
using SweetSugar.Scripts.TargetScripts.TargetSystem;
using UnityEngine;

namespace SweetSugar.Scripts.Level
{
    [Serializable]
    public class LevelData
    {
        public string Name;
        public static LevelData THIS;
        public int levelNum;
        public List<FieldData> fields = new List<FieldData>();
        [SerializeField] public Target targetObject;
        public int targetIndex;
        public LIMIT limitType;
        public int[] ingrCountTarget = new int[2];
        public int limit = 25;
        public int colorLimit = 5;
        public int star1 = 100;
        public int star2 = 300;
        public int star3 = 500;
        public int maxRows { get { return GetField().maxRows; } set { GetField().maxRows = value; } }
        public int maxCols { get { return GetField().maxCols; } set { GetField().maxCols = value; } }
        public int selectedTutorial;
        public int currentSublevelIndex;
        public List<SubTargetContainer> subTargetsContainers = new List<SubTargetContainer>();
        public FieldData GetField()
        {
            return fields[currentSublevelIndex];
        }
        public FieldData GetField(int index)
        {
            currentSublevelIndex = index;
            return fields[index];
        }
        public LevelData(bool isPlaying, int currentLevel)
        {
            levelNum = currentLevel;
            Name = "Level " + levelNum;
        }
        public Target GetTargetObject()
        {
            return targetObject;
        }
        public SquareBlocks GetBlock(int row, int col)
        {
            return GetField().levelSquares[row * GetField().maxCols + col];
        }
        public SquareBlocks GetBlock(Vector2Int vec)
        {
            return GetBlock(vec.y, vec.x);
        }
        public FieldData AddNewField()
        {
            var fieldData = new FieldData();
            fields.Add(fieldData);
            return fieldData;
        }
        public void RemoveField()
        {
            FieldData field = fields.Last();
            fields.Remove(field);
        }
  
        public string GetSaveString()
        {
            var str = "";
            foreach (var item in subTargetsContainers)
            {
                str += item.GetCount() + "/";
            }
            return str;
        }
        public LevelData DeepCopy(int level)
        {
            var other = (LevelData)MemberwiseClone();
            other.levelNum = level;
            other.Name = "Level " + other.levelNum;
            other.fields = new List<FieldData>();
            for (var i = 0; i < fields.Count; i++)
            {
                other.fields.Add(fields[i].DeepCopy());
            }
            if (targetObject != null)
                other.targetObject = targetObject.DeepCopy();
            other.subTargetsContainers = new List<SubTargetContainer>();
            for (var i = 0; i < subTargetsContainers.Count; i++)
            {
                other.subTargetsContainers.Add(subTargetsContainers[i].DeepCopy());
            }
            return other;
        }
        public LevelData DeepCopyForPlay(int level)
        {
            LevelData data = DeepCopy(level);
            data.subTargetsContainers = new List<SubTargetContainer>();
            for (var i = 0; i < subTargetsContainers.Count; i++)
            {
                subTargetsContainers[i].color = i;
                if (subTargetsContainers[i].GetCount() > 0)
                {
                    var subTargetContainer = subTargetsContainers[i].DeepCopy();
                    subTargetContainer.color = i;
                    data.subTargetsContainers.Add(subTargetContainer);
                }
            }

            return data;
        }
    }

    /// <summary>
    /// Field data contains field size and square array
    /// </summary>
    [Serializable]
    public class FieldData
    {
        public int subLevel;
        public int maxRows;
        public int maxCols;
        public bool noRegenLevel; //no regenerate level if no matches possible
        public SquareBlocks[] levelSquares = new SquareBlocks[81];
        internal int row;
        public int bombTimer = 15;
        public FieldData DeepCopy()
        {
            var other = (FieldData)MemberwiseClone();
            other.levelSquares = new SquareBlocks[levelSquares.Length];
            for (var i = 0; i < levelSquares.Length; i++)
            {
                other.levelSquares[i] = levelSquares[i].DeepCopy();
            }
            return other;
        }
    }

    /// <summary>
    /// Square blocks uses in editor
    /// </summary>
    [Serializable]
    public class SquareBlocks
    {
        public SquareTypes obstacle;
        public Vector2Int position;
        public SquareBlocks DeepCopy()
        {
            var other = (SquareBlocks)MemberwiseClone();
            return other;
        }
    }
}