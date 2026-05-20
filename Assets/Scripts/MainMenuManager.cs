using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager _ ;

    [SerializeField] private bool _debugMode;
    [SerializeField] private LevelDatabase levelDatabase;

    public enum MainMenuButtons { play, quit }

    private void Awake()
    {
        if (_ == null) _ = this;
        else Debug.LogError("More than one MainMenuManager in scene");
    }

    public void MainMenuButtonClicked(MainMenuButtons buttonClicked)
    {
        debugMessage("Button Clicked: " + buttonClicked);
        switch (buttonClicked)
        {
            case MainMenuButtons.play: playGame(); break;
            case MainMenuButtons.quit: quitGame(); break;
            default: Debug.Log("Unhandled button in MainMenuManager"); break;
        }
    }

    public void playGame()
    {
        if (levelDatabase != null)
        {
            LevelData level = levelDatabase.GetFirstAvailableLevel();
            if (level != null)
            {
                level.attempts++;
                SceneManager.LoadScene(level.sceneName);
                return;
            }
            Debug.LogWarning("No available levels found in database, falling back to build index 1");
        }
        // Fallback: load the first level by build index (index 0 = Main Menu)
        SceneManager.LoadScene(1);
    }

    public void quitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }

    private void debugMessage(string message)
    {
        if (_debugMode) Debug.Log(message);
    }
}
