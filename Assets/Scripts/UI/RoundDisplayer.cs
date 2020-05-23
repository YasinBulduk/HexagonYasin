using UnityEngine;
using TMPro;

public class RoundDisplayer : MonoBehaviour
{
    public TextMeshProUGUI roundText;

    public void UpdateRoundCount(int roundCount)
    {
        roundText.text = roundCount.ToString();
    }
}
