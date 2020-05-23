using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObservable
{
    void Register(Action action);
    void Unregister(Action action);
    void Notify();
}

public interface IObservable<T>
{
    void Register(Action<T> action);
    void Unregister(Action<T> action);
    void Notify();
}
