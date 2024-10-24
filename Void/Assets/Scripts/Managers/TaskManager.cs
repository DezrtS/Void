using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : Singleton<TaskManager>
{
    private List<Task> tasks = new List<Task>();

    private void OnEnable()
    {
        foreach (Task task in tasks)
        {
            task.OnTaskCompletion += OnTaskCompletion;
            task.OnSubTaskCompletion += OnSubtaskCompletion;
        }
    }

    private void OnDisable()
    {
        foreach (Task task in tasks)
        {
            task.OnTaskCompletion -= OnTaskCompletion;
            task.OnSubTaskCompletion -= OnSubtaskCompletion;
        }
    }

    public void AddTask(Task task)
    {
        if (!tasks.Contains(task))
        {
            tasks.Add(task);
            // OnTaskAdded Event
            task.OnTaskCompletion += OnTaskCompletion;
            task.OnSubTaskCompletion += OnSubtaskCompletion;
        }
    }

    public void RemoveTask(Task task)
    {
        if (tasks.Contains(task))
        {
            tasks.Remove(task);
            // OnTaskRemoved Event
            task.OnTaskCompletion -= OnTaskCompletion;
            task.OnSubTaskCompletion -= OnSubtaskCompletion;
        }
    }

    public void OnTaskCompletion(Task task)
    {
        Debug.Log($"{task.Data.Name} was completed");
        // Send Out Event That Other Observers Can Listen To.
    }

    public void OnSubtaskCompletion(Task task)
    {
        // Send Out Event That Other Observers Can Listen To.
    }
}