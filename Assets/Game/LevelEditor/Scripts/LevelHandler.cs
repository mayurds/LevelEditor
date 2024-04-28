using UnityEngine;
public class LevelHandler : MonoBehaviour
{
    public int currentLevel = 1;
    public int currentSubLevel = 0;
    public LevelData levelData;
    private void OnEnable()
    {
        levelData = LevelUtils.LoadForPlay(currentLevel);
        if (levelData.fields.Count > 0)
        {
            LevelField levelfield = new GameObject().AddComponent<LevelField>();
            levelfield.transform.parent = transform;
            levelfield.SetLevel(levelData, currentSubLevel);
        }
    }
}