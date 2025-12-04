using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Button AITurnButton;

    public void OnPauseButtonPressed()
    {
        pauseMenuUI.SetActive(true);
        AITurnButton.interactable = false;
        Time.timeScale = 0f;
    }
    public void OnResumeButtonPressed()
    {
        pauseMenuUI.SetActive(false);
        AITurnButton.interactable = true;
        Time.timeScale = 1f;
    }

    public void OnRestartButtonPressed()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void OnMainMenuButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
