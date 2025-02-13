using System.Linq;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    public enum SpawnpointType
    {
        Any,
        Survivor,
        Monster,
        Spectator,
        Item,
        Draggable,
        VoidBeast
    }

    [SerializeField] private SpawnpointType[] allowedSpawnpointTypes = new SpawnpointType[] { SpawnpointType.Any };

    private void Awake()
    {
        if (SpawnManager.Instance)
        {
            SpawnManager.Instance.AddSpawnpoint(this);
        }
        else
        {
            SpawnManager.OnSingletonInitialized += (SpawnManager spawnManager) =>
            {
                spawnManager.AddSpawnpoint(this);
            };
        }
    }

    public virtual bool CanSpawn(SpawnpointType spawnpointType)
    {
        return allowedSpawnpointTypes.Contains(spawnpointType);
    }

    public virtual Vector3 Spawn()
    {
        return transform.position;
    }
}