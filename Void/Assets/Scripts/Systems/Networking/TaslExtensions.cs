using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;

public static class TaskExtensions
{
    public static void Forget(this Task task)
    {
        task.ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                Debug.LogError(t.Exception);
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}

