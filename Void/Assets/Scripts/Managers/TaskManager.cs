using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TaskManager : NetworkSingleton<TaskManager>
{
    private NetworkVariable<int> totalTasks = new NetworkVariable<int>(0);
    private List<Task> tasks = new List<Task>();
    [SerializeField] private TextMeshProUGUI taskText;

    protected override void OnEnable()
    {
        base.OnEnable();
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
            totalTasks.Value++;
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
            totalTasks.Value--;
            // OnTaskRemoved Event
            task.OnTaskCompletion -= OnTaskCompletion;
            task.OnSubTaskCompletion -= OnSubtaskCompletion;
        }
    }

    public void OnTaskCompletion(Task task)
    {
        Debug.Log($"{task.Data.Name} was completed");
        TaskList.Instance.ClearTasks();
        DisplayTaskUI();
        //AudioManager.Instance.PlayOneShot(FMODEventManager.Instance.Sound1);
        // Send Out Event That Other Observers Can Listen To.
    }

    public void OnSubtaskCompletion(Task task)
    {
        // Send Out Event That Other Observers Can Listen To.

        TaskList.Instance.ClearTasks();
        DisplayTaskUI();
    }

    public void DisplayTaskUI()
    {
        foreach (var task in tasks)
        {
            TaskList.Instance.AddTask(task);
        }
    }
}