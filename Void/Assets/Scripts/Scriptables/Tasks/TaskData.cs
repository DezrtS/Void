using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TaskData", menuName = "ScriptableObjects/Task", order = 1)]
public class TaskData : ScriptableObject
{
    public enum TaskCompletionType
    {
        Binary,
        Percentage,
        Amount
    }

    [SerializeField] private string taskName;
    [SerializeField] private TaskCompletionType taskCompletionType;
    [SerializeField] private string taskInstructions;
    [SerializeField] private List<SubtaskData> subtasks;
    [SerializeField] private List<GameObject> taskPrefabs;

    public string TaskName => taskName;
    public TaskCompletionType CompletionType => taskCompletionType;
    public string TaskInstructions => taskInstructions;
    public List<SubtaskData> Subtasks => subtasks;
    public List<GameObject> TaskPrefabs => taskPrefabs;
}