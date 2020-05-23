using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICounter
{
    int Count { get; }

    void IncreaseCount();
    void DecreaseCount();
    void ResetCount();
}

public class RoundCounter : MonoBehaviour, ICounter
{
    public int Count { get; private set; }

    public Action OnRoundCountChanged { get; set; }
    public Action<int> OnRoundCountChangedInt { get; set; }

    public void DecreaseCount()
    {
        Count--;
        OnRoundCountChanged?.Invoke();
        OnRoundCountChangedInt?.Invoke(Count);
    }

    public void IncreaseCount()
    {
        Count++;
        OnRoundCountChanged?.Invoke();
        OnRoundCountChangedInt?.Invoke(Count);
    }

    public void ResetCount()
    {
        Count = 0;
        OnRoundCountChanged?.Invoke();
        OnRoundCountChangedInt?.Invoke(Count);
    }
}
