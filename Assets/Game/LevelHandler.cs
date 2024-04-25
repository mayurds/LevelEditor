using UnityEngine;
public class LevelHandler : MonoBehaviour
{
    public int currentLevel = 1;
    public int currentSubLevel = 0;
    public LevelData levelData;
    private void OnEnable()
    {
        levelData = LoadingManager.LoadForPlay(currentLevel);
        FieldData fieldData = new FieldData();
        if (levelData.fields.Count > 0)
        {
            fieldData = levelData.fields[currentSubLevel].DeepCopy();
            LevelField levelfield = new GameObject().AddComponent<LevelField>();
            levelfield.transform.parent = transform;
            Vector3 pos = GetCenterPosition(fieldData.maxCols, fieldData.maxRows);
            levelfield.transform.localPosition = pos;
            for (int k = 0; k < fieldData.levelSquares.Length; k++)
            {
                SquareBlocks squareBlock = fieldData.levelSquares[k];
                GameObject gameObject = new GameObject(squareBlock.obstacle.ToString());
                gameObject.transform.parent = levelfield.transform;
                gameObject.transform.localPosition = new Vector3(squareBlock.position.x, squareBlock.position.y, 0);
            }
            levelfield.transform.position = Vector3.zero;
        }
    }
    Vector3 GetCenterPosition(int col ,int row)
    {
        Vector3 vector3 = new Vector3(col, row, 0);
        vector3 = vector3 / 2;
        vector3 = new Vector3(vector3.x - (col % 2 == 0 ? 0 : 0.5f), vector3.y - (row % 2 == 0 ? 0 : 0.5f), 0);
        return vector3;
    }
}