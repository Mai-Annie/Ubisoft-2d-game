using UnityEngine;

[CreateAssetMenu(menuName = "Bamboo Bandits/Level Data")]
public class LevelData : ScriptableObject
{
    public string sceneName;
    public bool isAvailable = true;
    public bool isCompleted;
    public int attempts;
    public int highScore;
}
