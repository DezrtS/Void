using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DropOffObjectSubtaskData", menuName = "Scriptable Objects/DropOffObjectSubtaskData")]
public class DropOffObjectSubtaskData : SubtaskData
{
    public override ISubtask CreateSubtaskInstance(Task task, TaskData taskData)
    {
        DropOffObjectSubtask dropOffObjectSubtask = new DropOffObjectSubtask(taskData, this);
        dropOffObjectSubtask.Initialize(task);
        return dropOffObjectSubtask;
    }
}

public class DropOffObjectSubtask : Subtask
{
    private Draggable draggable;

    public DropOffObjectSubtask(TaskData taskData, SubtaskData subtaskData) : base(taskData, subtaskData)
    {

    }

    public override void Initialize(Task task)
    {
        base.Initialize(task);
        DragObjectTask dragObjectTask = task as DragObjectTask;

        if (dragObjectTask)
        {
            dragObjectTask.OnDraggable += OnDraggable;
        }
    }

    public override string GetSubtaskInstructions()
    {
        if (!draggable) return subtaskData.SubtaskInstructions;

        (string newInstructions, Dictionary<int, string> keys) = GetFormatInstructions(subtaskData.SubtaskInstructions);

        foreach (KeyValuePair<int, string> key in keys)
        {
            if (key.Value == "OBJECT")
            {
                newInstructions = newInstructions.Insert(key.Key, draggable.name.ToLower());
            }
        }

        return newInstructions;
    }

    public void OnDropOff(Draggable draggable, DraggableDropOff draggableDropOff)
    {
        if (this.draggable == draggable)
        {
            Execute();
            draggableDropOff.AcceptDraggable(draggable);
        }
    }

    public void OnDraggable(Draggable draggable)
    {
        this.draggable = draggable;
        DraggableDropOff.OnDropOff += OnDropOff;
        UpdateSubtaskInstructions();
    }
}
