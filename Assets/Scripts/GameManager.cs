using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private static bool shouldSkipMenu = false;
    private bool isGameOver = false;
    private static bool canJump = false;

    [Header("UI References")]
    public GameObject gameOverPanel;
    public GameObject mainMenuPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverPanelText;

    [Header("Game Over Effects")]
    public float gameOverDelay = 1f;

    [Header("Camera Settings")]
    public float cameraShakeDuration = 0.5f;
    public float cameraShakeIntensity = 0.3f;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        mainMenuPanel.SetActive(!shouldSkipMenu);
        gameOverPanel.SetActive(false);
        Time.timeScale = shouldSkipMenu ? 1f : 0f;
        if (shouldSkipMenu)
        {
            shouldSkipMenu = false;
        }
    }

    public void StartGame()
    {
        Debug.Log("START GAME");
        mainMenuPanel.SetActive(false);
        isGameOver = false;
        StartCoroutine(DelayedTimeScaleReset());
    }

    private IEnumerator DelayedTimeScaleReset()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        Time.timeScale = 1f;
        canJump = true;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void GameOver()
    {
        Debug.Log("<color=magenta>GameOver</color>");
        if (isGameOver) return;

        canJump = false;
        isGameOver = true;

        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        Time.timeScale = 0.5f;

        SmartCamera[] cameras = FindObjectsOfType<SmartCamera>();
        foreach (SmartCamera cam in cameras)
        {
            cam.TriggerShake(0.5f, 0.3f);
        }

        yield return new WaitForSecondsRealtime(gameOverDelay);

        Time.timeScale = 0f;
        ShowGameOverPanel();
    }


    private void ShowGameOverPanel()
    {
        if (ScoreManager.Instance != null)
        {
            scoreText.text = "Score: " + ScoreManager.Instance.GetScore();
        }
        gameOverPanelText.text = MasterSpawner.Instance.gameCompleted ? GameText.GameCompleted : GameText.GameOver;
        gameOverPanel.SetActive(true);

    }

    public void RestartGame()
    {
        Debug.Log("RESTART GAME");
        shouldSkipMenu = true;
        canJump = true;
        MasterSpawner.Instance.gameCompleted = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        Debug.Log("BackToMainMenu");
        shouldSkipMenu = false;
        MasterSpawner.Instance.gameCompleted = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CompleteGame()
    {
        if (isGameOver) return;

        canJump = false;
        isGameOver = true;
        ShowGameOverPanel();
    }

    public bool IsGameOver() => isGameOver;
    public bool CanJump() => canJump;
    public void SetCanJump(bool state)
    {
        canJump = state;
        Debug.Log($"Jumping {(state ? "enabled" : "disabled")}");
    }

    public static class GameText
    {
        public const string GameOver = "GAME OVER!";
        public const string GameCompleted = "GAME COMPLETED!";
    }
}