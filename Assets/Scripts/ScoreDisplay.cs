using System;
using Level;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _highScoreText;
    [SerializeField] private TextMeshProUGUI _currentScoreText;

    private void DisplayScore(int currentScore, int highScore)
    {
        _currentScoreText.text = currentScore.ToString();
        _highScoreText.text = highScore.ToString();
    }

    private GameManager _gameManager;
    private void OnEnable()
    {
        _gameManager = GameManager.Instance;

        _gameManager.OnUpdateScore += DisplayScore;
    }

    private void OnDisable()
    {
        _gameManager.OnUpdateScore -= DisplayScore;
    }
}
