using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [SerializeField] private bool debugMode;
    [SerializeField] private LevelDatabase levelDatabase;

    public enum MenuButton { Play, Quit }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnMenuButtonClicked(MenuButton button)
    {
        if (debugMode) Debug.Log("Button clicked: " + button);
        switch (button)
        {
            case MenuButton.Play: PlayGame(); break;
            case MenuButton.Quit: QuitGame(); break;
            default: Debug.LogWarning("Unhandled MenuButton: " + button); break;
        }
    }

    public void PlayGame()
    {
        if (levelDatabase != null)
        {
            LevelData level = levelDatabase.GetFirstAvailableLevel();
            if (level != null)
            {
                level.RecordAttempt();
                SceneManager.LoadScene(level.sceneName);
                return;
            }
            Debug.LogWarning("No available levels in database — falling back to build index 1");
        }
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
