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
                Vector2 pos = new Vector2(0, 0);
                int mapSize = Mathf.Max(fieldData.maxCols, fieldData.maxRows);
                float hexRadius = 0.5f;
                for (int q = -mapSize; q <= mapSize; q++)
                {
                    int r1 = Mathf.Max(-mapSize, -q - mapSize);
                    int r2 = Mathf.Min(mapSize, -q + mapSize);
                    for (int r = r1; r <= r2; r++)
                    {
                        pos.x = hexRadius * Mathf.Sqrt(3.0f) * (q + r / 2.0f);
                        pos.y = hexRadius * 3.0f / 2.0f * r;
                        SquareBlocks squareBlock = levelData.GetField(subLevel).GetHex(new Vector3Int(q, r, -q - r));
                        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        gameObject.name = squareBlock.obstacle.ToString();
                        gameObject.transform.position = new Vector3(pos.x,0 ,pos.y);
                    }
                }
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
