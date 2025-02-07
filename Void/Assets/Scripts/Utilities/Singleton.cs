using System;
using Unity.Netcode;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour, ISingleton<T> where T : MonoBehaviour
{
    private static T instance;
    public static T Instance => instance;

    public static event Action<T> OnSingletonInitialized;

    protected virtual void OnEnable()
    {
        InitializeSingleton();
    }

    public virtual void InitializeSingleton()
    {
        if (instance != null)
        {
            Debug.LogWarning($"There were multiple instances of {name} in the scene");

            Destroy(gameObject);
            return;
        }

        instance = this as T;
        OnSingletonInitialized?.Invoke(instance);
    }
}

public abstract class NetworkSingleton<T> : NetworkBehaviour, ISingleton<T> where T : NetworkBehaviour
{
    private static T instance;
    public static T Instance => instance;

    public static event Action<T> OnSingletonInitialized;

    protected virtual void OnEnable()
    {
        InitializeSingleton();
    }

    public virtual void InitializeSingleton()
    {
        if (instance != null)
        {
            Debug.LogWarning($"There were multiple instances of {name} in the scene");

            Destroy(gameObject);
            return;
        }

        instance = this as T;
        OnSingletonInitialized?.Invoke(instance);
    }
}

public abstract class SingletonPersistent<T> : Singleton<T> where T : MonoBehaviour, ISingleton<T>
{
    public override void InitializeSingleton()
    {
        base.InitializeSingleton();
        DontDestroyOnLoad(gameObject);
    }
}

public abstract class NetworkSingletonPersistent<T> : NetworkSingleton<T> where T : NetworkBehaviour, ISingleton<T>
{
    public override void InitializeSingleton()
    {
        base.InitializeSingleton();
        DontDestroyOnLoad(gameObject);
    }
}