using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public GameObject enemyPrefab;
    public int enemyCount = 5;

    [Header("UI References")]
    public GameObject gameUI;
    public GameObject pauseUI;
    public GameObject endUI;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button backToMainButton;
    public Button endRestartButton;
    public Button endBackToMainButton;

    private List<Enemy> enemies = new List<Enemy>();
    private Player player;
    private bool gameActive = false;
    private bool gamePaused = false;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        SetupUI();
        StartGame();
    }

    void SetupUI()
    {
        pauseButton.onClick.AddListener(PauseGame);
        resumeButton.onClick.AddListener(ResumeGame);
        restartButton.onClick.AddListener(RestartGame);
        backToMainButton.onClick.AddListener(BackToMainMenu);
        endRestartButton.onClick.AddListener(RestartGame);
        endBackToMainButton.onClick.AddListener(BackToMainMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameActive)
        {
            if (gamePaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void StartGame()
    {
        ClearGame();
        SpawnEnemies();

        gameActive = true;
        gamePaused = false;
        Time.timeScale = 1f;

        ShowGameUI();

        // Set camera target to player
        CameraController cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController != null && player != null)
        {
            cameraController.SetTarget(player.transform);
        }
    }

    void SpawnEnemies()
    {
        enemies.Clear();
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            enemies.Add(enemy);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        return new Vector3(x, 1f, z);
    }

    public void EnemyDestroyed(Enemy enemy)
    {
        enemies.Remove(enemy);
        if (enemies.Count == 0)
        {
            GameWon();
        }
    }

    void GameWon()
    {
        gameActive = false;
        Time.timeScale = 0f;
        ShowEndUI();
    }

    public void PauseGame()
    {
        if (!gameActive) return;
        gamePaused = true;
        Time.timeScale = 0f;
        ShowPauseUI();
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;
        ShowGameUI();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    void ClearGame()
    {
        gameActive = false;
        gamePaused = false;

        // Clear all game objects
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] lasers = GameObject.FindGameObjectsWithTag("Laser");

        foreach (GameObject obj in enemies) Destroy(obj);
        foreach (GameObject obj in lasers) Destroy(obj);

        this.enemies.Clear();
    }

    void ShowGameUI()
    {
        gameUI.SetActive(true);
        pauseUI.SetActive(false);
        endUI.SetActive(false);
    }

    void ShowPauseUI()
    {
        gameUI.SetActive(false);
        pauseUI.SetActive(true);
        endUI.SetActive(false);
    }

    void ShowEndUI()
    {
        gameUI.SetActive(false);
        pauseUI.SetActive(false);
        endUI.SetActive(true);
    }

    public bool IsGamePaused()
    {
        return gamePaused;
    }
}