using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Subtask
{
    public int Id;
    public string Name;
    public string Instructions;
}

public enum TaskCompletionType
{
    Binary,
    Percentage,
    Amount
}

[CreateAssetMenu(fileName = "TaskData", menuName = "ScriptableObjects/Task", order = 1)]
public class TaskData : ScriptableObject
{
    public string Name;
    public TaskCompletionType CompletionType;
    public string Description;
    public bool IsGlobal;
    public List<Subtask> Subtasks;
}