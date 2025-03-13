using UnityEngine;

[CreateAssetMenu(fileName = "AOEProjectileData", menuName = "ScriptableObjects/Projectiles/AOEProjectileData", order = 1)]
public class AOEProjectileData : ProjectileData
{
    [SerializeField] private float range;

    public float Range => range;
}