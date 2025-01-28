using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeliverItemSubtaskData", menuName = "Scriptable Objects/DeliverItemSubtaskData")]
public class DeliverItemSubtaskData : SubtaskData
{
    public override ISubtask CreateSubtaskInstance(Task task, TaskData taskData)
    {
        DeliverItemSubtask deliverItemSubtask = new DeliverItemSubtask(taskData, this);
        deliverItemSubtask.Initialize(task);
        return deliverItemSubtask;
    }
}

public class DeliverItemSubtask : Subtask
{
    private Item item;

    public DeliverItemSubtask(TaskData taskData, SubtaskData subtaskData) : base(taskData, subtaskData)
    {

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
        UpdateSubtaskInstructions();
    }
}
