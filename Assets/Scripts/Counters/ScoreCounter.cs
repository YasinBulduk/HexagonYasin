using System;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    public Action<int> OnHighScoreChanged { get; set; }
    public Action<int> OnCurrentScoreChanged { get; set; }
    public int CurrentScore { get; private set; }

    private const string m_PPHighScoreKey = "hs";
    private int m_HighScore;

    void Start()
    {
        GetHighScore();
        CurrentScore = 0;
    }

    private bool IsHighScoreExceeded()
    {
        return m_HighScore < CurrentScore;
    }

    private void GetHighScore()
    {
        SetHighScore(PlayerPrefs.GetInt(m_PPHighScoreKey, 0));
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(m_PPHighScoreKey, m_HighScore);
        PlayerPrefs.Save();
    }

    private void SetHighScore(int newScore)
    {
        m_HighScore = newScore;
        OnHighScoreChanged?.Invoke(m_HighScore);
        SaveHighScore();
    }

    public void IncreaseScore(int amount)
    {
        CurrentScore += amount;
        OnCurrentScoreChanged?.Invoke(CurrentScore);

        if(IsHighScoreExceeded())
        {
            SetHighScore(CurrentScore);
        }
    }

    public void DecreaseScore(int amount)
    {
        CurrentScore -= amount;
        OnCurrentScoreChanged?.Invoke(CurrentScore);
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        OnCurrentScoreChanged?.Invoke(CurrentScore);
    }
}
