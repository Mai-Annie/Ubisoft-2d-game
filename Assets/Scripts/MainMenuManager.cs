using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour //extends MonoBehaviour
{
    public static MainMenuManager _; //Singleton instance
    private void Awake()
    {
        if (_ == null)
        {
            _ = this;
        }
        else
        {
            Debug.LogError("There are more than 1 MainMenuManager's in the scene");
        }
    }

    [SerializeField] private bool _debugMode; //prevents other scripts from modifying it
    public enum MainMenuButtons { play, quit };
    public void MainMenuButtonClicked(MainMenuButtons buttonClicked)
    {
        debugMessage("Button Clicked:" + buttonClicked.ToString());
        switch (buttonClicked)
        {
            case MainMenuButtons.play:
                playGame();
                break;
            case MainMenuButtons.quit:
                quitGame();
                break;
            default:
                Debug.Log("Button clicked not handled in MainMenuManager");
                break;
        }
    }
    private void debugMessage( string message)
    {
        if (_debugMode)
        {
            Debug.Log(message);
        }
    }
    public void playGame()
    {
        SceneManager.LoadScene("Level 1");
    }
    public void quitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }
}
