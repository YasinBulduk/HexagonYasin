using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public ScoreDisplayer scoreDisplayer;
    public RoundDisplayer roundDisplayer;
    public DropdownButton dropdownMenu;
    public MessageDisplayer messageDisplayer;

    private void Start()
    {
        scoreDisplayer.UpdateCurrentScore(0);
        roundDisplayer.UpdateRoundCount(0);
    }

    public void DisplayReplayButton()
    {
        if (!dropdownMenu.IsMenuOpen)
        {
            dropdownMenu.GetComponent<Button>().onClick.Invoke();
        }
    }
}
