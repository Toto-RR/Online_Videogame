using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> actionQueue = new Queue<Action>();

    void Update()
    {
        lock (actionQueue)
        {
            while (actionQueue.Count > 0)
            {
                actionQueue.Dequeue()?.Invoke();
            }
        }
    }

    public static void RunOnMainThread(Action action)
    {
        if (action == null) return;
        lock (actionQueue)
        {
            actionQueue.Enqueue(action);
        }
    }
}
