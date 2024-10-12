using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Creates a private static generic variable for the instance of the singleton
    private static T instance;

    // Creates a static generic field to retrieve the private instance variable
    public static T Instance { get { return instance; } }

    protected virtual void Awake()
    {
        // Destroys the current instance that runs this Awake() function if there is another instance of this class in the scene
        if (instance != null)
        {
            Debug.LogWarning($"There were multiple instances of {name} in the scene");

            Destroy(gameObject);
            return;
        }

        instance = this as T;
    }
}

public abstract class SingletonPersistent<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}