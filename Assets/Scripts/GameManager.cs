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

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
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
        CompleteLevel();
    }

    private void CompleteLevel()
    {
        levelActive = false;

        if (levelDatabase != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            levelDatabase.GetLevelByScene(currentScene)?.RecordCompletion(score);
            LevelData next = levelDatabase.GetNextLevel(currentScene);
            if (next != null) next.isAvailable = true;
        }

        HUDManager.Instance?.ShowMessage("Level Complete!");
        Invoke(nameof(LoadNextScene), 2f);
    }

    private void OnTimeOut()
    {
        levelActive = false;

        if (levelDatabase != null)
            levelDatabase.GetLevelByScene(SceneManager.GetActiveScene().name)?.RecordAttempt();

        HUDManager.Instance?.ShowMessage("Time's Up!");
        Invoke(nameof(ReloadLevel), 2f);
    }

    private void LoadNextScene()
    {
        if (levelDatabase != null)
        {
            LevelData next = levelDatabase.GetNextLevel(SceneManager.GetActiveScene().name);
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
