using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DragObjectSubtaskData", menuName = "Scriptable Objects/DragObjectSubtaskData")]
public class DragObjectSubtaskData : SubtaskData
{
    [SerializeField] private bool undoOnStopDragging = true;

    public override ISubtask CreateSubtaskInstance(Task task, TaskData taskData)
    {
        DragObjectSubtask dragObjectSubtask = new DragObjectSubtask(taskData, this, undoOnStopDragging);
        dragObjectSubtask.Initialize(task);
        return dragObjectSubtask;
    }
}

public class DragObjectSubtask : Subtask
{
    private readonly bool undoOnStopDragging;
    private Draggable draggable;

    public DragObjectSubtask(TaskData taskData, SubtaskData subtaskData, bool undoOnStopDragging) : base(taskData, subtaskData)
    {
        this.undoOnStopDragging = undoOnStopDragging;
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

    public void OnDraggable(Draggable draggable)
    {
        this.draggable = draggable;
        draggable.OnUsed += OnUseableStateChanged;
        RequestRegenerateTaskInstructions();
    }

    public void OnUseableStateChanged(IUseable useable, bool isUsing)
    {
        if (isUsing)
        {
            if (isCompleted) return;
            Execute();
        }
        else
        {
            if (!isCompleted || !undoOnStopDragging) return;
            Undo();
        }
    }
}
