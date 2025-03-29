using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private int score;
    public TextMeshProUGUI scoreText;

    void Awake()
    {
        Instance = this;
    }

    public void AddScore(int points)
    {
        if (GameManager.Instance.IsGameOver()) return;

        score += points;
        scoreText.text = "SCORE: " + score;
    }

    public int GetScore() => score;
}