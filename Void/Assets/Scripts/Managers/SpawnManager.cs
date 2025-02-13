using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    private readonly List<Spawnpoint> spawnpoints = new List<Spawnpoint>();

    public void AddSpawnpoint(Spawnpoint spawnpoint)
    {
        spawnpoints.Add(spawnpoint);
    }

    public List<Spawnpoint> GetSpawnpoints(Spawnpoint.SpawnpointType spawnpointType)
    {
        return spawnpoints.FindAll(spawnpoint => spawnpoint.CanSpawn(spawnpointType));
    }

    public Spawnpoint GetRandomSpawnpoint(Spawnpoint.SpawnpointType spawnpointType)
    {
        List<Spawnpoint> spawnpoints = GetSpawnpoints(spawnpointType);

        if (spawnpoints.Count > 0)
        {
            int index = Random.Range(0, spawnpoints.Count);
            return spawnpoints[index];
        }

        Debug.LogWarning($"No available {spawnpointType} spawnpoints were found");
        return null;
    }

    public Vector3 GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType spawnpointType)
    {
        Spawnpoint spawnpoint = GetRandomSpawnpoint(spawnpointType);

        if (spawnpoint == null) return Vector3.zero;
        return spawnpoint.Spawn();
    }
}
