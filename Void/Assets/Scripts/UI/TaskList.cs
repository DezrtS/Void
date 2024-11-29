using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskList : Singleton<TaskList>
{
    [SerializeField] private TextMeshProUGUI taskText;

    public void AddTask(Task task)
    {
        Debug.Log("ADDED TEXT");
        taskText.text += task.Data.Name + "\n" + task.Data.Description + "\n\n";
    }

    public void ClearTasks()
    {
        taskText.text = "";
    }
}
