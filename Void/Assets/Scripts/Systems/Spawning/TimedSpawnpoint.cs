using UnityEngine;

public class TimedSpawnpoint : Spawnpoint
{
    [SerializeField] private float delayAfterSpawn;
    private float timeUntilNextSpawn;

    public override bool CanSpawn(SpawnpointType spawnpointType)
    {
        if (Time.timeSinceLevelLoad < timeUntilNextSpawn) return false;

        return base.CanSpawn(spawnpointType);
    }

    public override Vector3 Spawn()
    {
        timeUntilNextSpawn = Time.timeSinceLevelLoad + delayAfterSpawn;
        return base.Spawn();
    }
}