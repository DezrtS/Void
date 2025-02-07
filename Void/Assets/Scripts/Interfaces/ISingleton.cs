using System;

public interface ISingleton<T>
{
    public static T Instance { get; }
    public abstract void InitializeSingleton();
}