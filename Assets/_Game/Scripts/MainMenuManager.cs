using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    

    public void OnStartButtonPressed()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }
}
