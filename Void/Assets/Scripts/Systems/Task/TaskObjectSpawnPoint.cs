using UnityEngine;

public class TaskObjectSpawnPoint : MonoBehaviour
{
    public enum TaskObjectType
    {
        Item,
        Draggable,
    }

    [SerializeField] private TaskObjectType objectType;
    private bool occupied = false;
    private GameObject taskObject;

    public TaskObjectType ObjectType => objectType;
    public bool IsOccupied => occupied;

    private void Awake()
    {
        TaskManager.OnSingletonInitialized += (TaskManager taskManager) =>
        {
            taskManager.AddTaskObjectSpawnPoint(this);
        };
    }

    public void AddObject(GameObject gameObject)
    {
        taskObject = gameObject;
        occupied = true;
    }
}