using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    [Serializable]
    public class LevelData
    {
        public string Name;
        public static LevelData THIS;
        public int levelNum;
        public List<FieldData> fields = new List<FieldData>();
        public GridDirection gridDirection;
        public GridType gridType;
        public int maxRows { get { return GetField().maxRows; } set { GetField().maxRows = value; } }
        public int maxCols { get { return GetField().maxCols; } set { GetField().maxCols = value; } }
        public int currentSublevelIndex;
        public FieldData GetField()
        {
            return fields[currentSublevelIndex];
        }

        public FieldData GetField(int index)
        {
            currentSublevelIndex = index;
            return fields[index];
        }
        public LevelData(int currentLevel)
        {
            levelNum = currentLevel;
            Name = "Level " + levelNum;
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
            return other;
        }
        public LevelData DeepCopyForPlay(int level)
        {
            LevelData data = DeepCopy(level);

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
        public SquareBlocks[] levelSquares = new SquareBlocks[81];
        internal int row;
    public SquareBlocks GetHex(Vector2 pos)
    {
        return levelSquares.First(f => f.position == pos);
    }
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
        public Vector2 position;
        public SquareBlocks DeepCopy()
        {
            var other = (SquareBlocks)MemberwiseClone();
            return other;
        }
    }


    public enum SquareTypes
    {
        NONE,
        EmptySquare
    }

public enum GridDirection
{
    XY,
    XZ
}

public enum GridType
{
    Square,
    Hex
}