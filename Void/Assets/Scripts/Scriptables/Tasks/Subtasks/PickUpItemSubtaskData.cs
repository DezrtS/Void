using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PickUpItemSubtaskData", menuName = "Scriptable Objects/PickUpItemSubtaskData")]
public class PickUpItemSubtaskData : SubtaskData
{
    [SerializeField] private bool undoOnDrop = true;

    public override ISubtask CreateSubtaskInstance(Task task, TaskData taskData)
    {
        PickUpItemSubtask pickUpItemSubtask = new PickUpItemSubtask(taskData, this, undoOnDrop);
        pickUpItemSubtask.Initialize(task);
        return pickUpItemSubtask;
    }
}

public class PickUpItemSubtask : Subtask
{
    private readonly bool undoOnDrop;
    private Item item;

    public PickUpItemSubtask(TaskData taskData, SubtaskData subtaskData, bool undoOnDrop) : base(taskData, subtaskData)
    {
        this.undoOnDrop = undoOnDrop;
    }

    public override void Initialize(Task task)
    {
        base.Initialize(task);
        ItemRetrievalTask itemRetrievalTask = task as ItemRetrievalTask;

        if (itemRetrievalTask)
        {
            itemRetrievalTask.OnItem += OnItem;
        }
    }

    public override string GetSubtaskInstructions()
    {
        if (!item) return subtaskData.SubtaskInstructions;

        (string newInstructions, Dictionary<int, string> keys) = GetFormatInstructions(subtaskData.SubtaskInstructions);

        foreach (KeyValuePair<int, string> key in keys)
        {
            if (key.Value == "ITEM")
            {
                newInstructions = newInstructions.Insert(key.Key, item.ItemData.Name.ToLower());
            }
        }

        return newInstructions;
     }

    public void OnItem(Item item)
    {
        this.item = item;
        item.OnPickUp += OnItemPickUpStateChanged;
        RequestRegenerateTaskInstructions();
    }

    public void OnItemPickUpStateChanged(Item item, bool pickedUp)
    {
        if (pickedUp)
        {
            if (isCompleted) return;
            Execute();
        }
        else
        {
            if (!isCompleted || !undoOnDrop) return;
            Undo();
        }
    }
}
