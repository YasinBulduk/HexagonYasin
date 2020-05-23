using UnityEngine;
using TMPro;

public class ScoreDisplayer : MonoBehaviour
{
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highScoreText;

    public void UpdateCurrentScore(int score)
    {
        currentScoreText.text = score.ToString();
    }

    public void UpdateHighScore(int score)
    {
        highScoreText.text = string.Format("Highscore: {0}", score);
    }
}
