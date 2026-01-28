using UnityEngine;

public class MainMenuButtonManager : MonoBehaviour
{
    [SerializeField] MainMenuManager.MainMenuButtons _buttonType;
    public void OnButtonClicked()
    {
        MainMenuManager._.MainMenuButtonClicked(_buttonType);
    }
}
