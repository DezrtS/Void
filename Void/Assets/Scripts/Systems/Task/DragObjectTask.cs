using System;
using UnityEngine;

public class DragObjectTask : Task
{
    [SerializeField] private GameObject draggablePrefab;

    public event Action<Draggable> OnDraggable;

    public GameObject DraggablePrefab => draggablePrefab;

    public void SpawnDraggable(Draggable draggable)
    {
        OnDraggable?.Invoke(draggable);
    }
}