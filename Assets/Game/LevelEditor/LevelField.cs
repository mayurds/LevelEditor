using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelField : MonoBehaviour
{
    public FieldData fieldData = new FieldData();
    public void SetLevel( LevelData levelData,int subLevel)
    {
        fieldData = levelData.fields[subLevel].DeepCopy();
        switch (levelData.gridType)
        {
            case GridType.Square:
                transform.localPosition = GetCenterPosition(fieldData.maxCols, fieldData.maxRows);
                for (int k = 0; k < fieldData.levelSquares.Length; k++)
                {
                    SquareBlocks squareBlock = fieldData.levelSquares[k];
                    GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    gameObject.transform.localPosition = new Vector3(squareBlock.position.x, 0, squareBlock.position.y);
                    gameObject.transform.parent = transform;
                }
                transform.position = Vector3.zero;
                break;
            case GridType.Hex:
                for (int k = 0; k < fieldData.levelSquares.Length; k++)
                {
                    SquareBlocks squareBlock = fieldData.levelSquares[k];
                    GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    switch (levelData.gridDirection)
                    {
                        case GridDirection.XY:
                            gameObject.transform.position = new Vector3(squareBlock.position.x, squareBlock.position.y,0 );
                            break;
                        case GridDirection.XZ:
                            gameObject.transform.position = new Vector3(squareBlock.position.x, 0, squareBlock.position.y);
                            break;
                    }
                    gameObject.transform.parent = transform;
                }
               /// transform.position = Vector3.zero;
                break;
        }
 
    }
    Vector3 GetCenterPosition(int col, int row)
    {
        Vector3 vector = new Vector3(col, row, 0);
        vector = vector / 2;
        vector = new Vector3(vector.x - 0.5f, 0, vector.y - 0.5f);
        return vector;
    }
}
