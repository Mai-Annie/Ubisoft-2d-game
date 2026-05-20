using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelDatabase levelDatabase;
    [SerializeField] private float levelTimeLimit = 60f;

    private float timeRemaining;
    private int score;
    private bool levelActive;

    public float TimeRemaining => timeRemaining;
    public int Score => score;
    public bool IsActive => levelActive;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        timeRemaining = levelTimeLimit;
        levelActive = true;
    }

    private void Update()
    {
        if (!levelActive) return;
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            OnTimeOut();
        }
    }

    public void OnBambooDelivered(int pointValue)
    {
        if (!levelActive) return;
        int timeBonus = Mathf.RoundToInt(timeRemaining);
        score += pointValue + timeBonus;
        Debug.Log($"Delivered! +{pointValue} pts + {timeBonus} time bonus = {score} total");
        CompleteLevel();
    }

    private void CompleteLevel()
    {
        levelActive = false;

        string currentScene = SceneManager.GetActiveScene().name;
        if (levelDatabase != null)
        {
            foreach (var level in levelDatabase.levels)
            {
                if (level.sceneName == currentScene)
                {
                    level.isCompleted = true;
                    if (score > level.highScore) level.highScore = score;
                    break;
                }
            }
            LevelData next = levelDatabase.GetNextLevel(currentScene);
            if (next != null) next.isAvailable = true;
        }

        HUDManager.Instance?.ShowMessage("Level Complete!");
        Invoke(nameof(LoadNextScene), 2f);
    }

    private void OnTimeOut()
    {
        levelActive = false;
        HUDManager.Instance?.ShowMessage("Time's Up!");
        Invoke(nameof(ReloadLevel), 2f);
    }

    private void LoadNextScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (levelDatabase != null)
        {
            LevelData next = levelDatabase.GetNextLevel(currentScene);
            if (next != null) { SceneManager.LoadScene(next.sceneName); return; }
        }
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            SceneManager.LoadScene("Main Menu");
    }

    private void ReloadLevel()
    {
        if (levelDatabase != null)
        {
            string current = SceneManager.GetActiveScene().name;
            foreach (var level in levelDatabase.levels)
            {
                if (level.sceneName == current) { level.attempts++; break; }
            }
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
