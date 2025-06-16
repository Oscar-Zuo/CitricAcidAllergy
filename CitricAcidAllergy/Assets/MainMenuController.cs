using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OnStartGameClicked()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OnNameChanged(string name)
    {
        PlayerPrefs.SetString("SenderName", name);
    }
}
