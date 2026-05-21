using UnityEngine;

[CreateAssetMenu(menuName = "Bamboo Bandits/Level Data")]
public class LevelData : ScriptableObject
{
    public string sceneName;
    public bool isAvailable = true;
    public bool isCompleted;
    public int attempts;
    public int highScore;

    public void RecordAttempt() => attempts++;

    public void RecordCompletion(int score)
    {
        isCompleted = true;
        if (score > highScore) highScore = score;
    }
}
