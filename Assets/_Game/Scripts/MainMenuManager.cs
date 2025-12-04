using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    

    public void OnStartButtonPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }
}
