using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Bamboo Bandits/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelData> levels = new List<LevelData>();

    public LevelData GetLevelByScene(string sceneName)
    {
        foreach (var level in levels)
            if (level.sceneName == sceneName) return level;
        return null;
    }

    public LevelData GetFirstAvailableLevel()
    {
        foreach (var level in levels)
            if (level.isAvailable && !level.isCompleted) return level;
        return levels.Count > 0 ? levels[levels.Count - 1] : null;
    }

    public LevelData GetNextLevel(string currentSceneName)
    {
        for (int i = 0; i < levels.Count - 1; i++)
            if (levels[i].sceneName == currentSceneName) return levels[i + 1];
        return null;
    }
}
