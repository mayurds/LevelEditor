using UnityEngine;

[CreateAssetMenu(fileName = "LevelContainer", menuName = "LevelContainer", order = 1)]
public class LevelContainer : ScriptableObject
{
    public LevelData levelData;
    public void SetData(LevelData levelData)
    {
        this.levelData = levelData;
    }
}
