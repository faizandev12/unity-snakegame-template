using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    private int score = 0;
    private int highScore = 0;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            highScore = PlayerPrefs.GetInt("HighScore", 0); // Load saved high score
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        UpdateScoreUI();
    }
    public void AddScore(int amount)
    {
        score += amount;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore); // Save new high score
        }
        UpdateScoreUI();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
    }

}
