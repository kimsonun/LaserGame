using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public Button quitButton;

    void Start()
    {
        SetupUI();
        Time.timeScale = 1f; // Ensure normal time scale
    }

    void SetupUI()
    {
        startButton.onClick.AddListener(StartGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}