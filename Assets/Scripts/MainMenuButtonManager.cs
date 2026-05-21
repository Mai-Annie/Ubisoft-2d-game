using UnityEngine;

public class MainMenuButtonManager : MonoBehaviour
{
    [SerializeField] private MainMenuManager.MenuButton buttonType;

    public void OnButtonClicked()
    {
        MainMenuManager.Instance.OnMenuButtonClicked(buttonType);
    }
}
