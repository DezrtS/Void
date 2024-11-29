using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryTask : Task
{
    private Item item;

    private void Start()
    {
        item = GetComponent<Item>();
        item.OnPickedUp += OnItemPickUp;
        item.OnDropped += OnItemDrop;
    }

    private void OnItemPickUp(Item item)
    {
        CompleteSubtask(0);
    }

    private void OnItemDrop(Item item)
    {

    }

    public override void CompleteTask()
    {
        base.CompleteTask();
        TaskManager.Instance.RemoveTask(this);
        item.OnPickedUp -= OnItemPickUp;
        item.OnDropped -= OnItemDrop;
        NetworkObject.Despawn();
    }
}