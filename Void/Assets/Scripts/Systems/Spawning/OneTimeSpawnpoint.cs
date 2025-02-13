using UnityEngine;

public class OneTimeSpawnpoint : Spawnpoint
{
    private bool spawnedAt;

    public override bool CanSpawn(SpawnpointType spawnpointType)
    {
        if (spawnedAt) return false;

        return base.CanSpawn(spawnpointType);
    }

    public override Vector3 Spawn()
    {
        spawnedAt = true;
        return base.Spawn();
    }
}