using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Generic Event", menuName = "Utils/Events/Generic Event")]
public class GameEvent : ScriptableObject
{
    public bool debugLog;

    private readonly List<GameEventListener> eventListeners = new();

    public Action action;

    public void Raise()
    {
        for (var i = eventListeners.Count - 1; i >= 0; i--)
        {
            eventListeners[i].OnEventRaised();
        }

        action?.Invoke();


        if (debugLog)
        {
            Debug.Log(name + " game event raised", this);
        }
    }

    public void RegisterListener(GameEventListener listener)
    {
        if (!eventListeners.Contains(listener))
        {
            eventListeners.Add(listener);
        }
    }

    public void UnregisterListener(GameEventListener listener)
    {
        if (eventListeners.Contains(listener))
        {
            eventListeners.Remove(listener);
        }
    }

    public void RegisterAction(Action callback)
    {
        action += callback;
    }

    public void UnregisterAction(Action callback)
    {
        action -= callback;
    }
}

public class GameEvent<T> : ScriptableObject
{
    public bool debugLog;

    public T testRaiseData;

    private readonly List<IGameEventListener<T>> eventListeners = new();

    public Action<T> action;

    public void Raise(T obj)
    {
        for (var i = eventListeners.Count - 1; i >= 0; i--)
        {
            eventListeners[i].OnEventRaised(obj);
        }

        action?.Invoke(obj);

        if (debugLog)
        {
            Debug.Log(name + " game event raised", this);
        }
    }

    public void RegisterListener(IGameEventListener<T> listener)
    {
        if (!eventListeners.Contains(listener))
        {
            eventListeners.Add(listener);
        }
    }

    public void UnregisterListener(IGameEventListener<T> listener)
    {
        if (eventListeners.Contains(listener))
        {
            eventListeners.Remove(listener);
        }
    }

    public void RegisterAction(Action<T> callback)
    {
        action += callback;
    }

    public void UnregisterAction(Action<T> callback)
    {
        action -= callback;
    }
}
