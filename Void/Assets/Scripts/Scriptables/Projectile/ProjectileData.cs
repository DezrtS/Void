using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "ScriptableObjects/Projectile", order = 1)]
public class ProjectileData : ScriptableObject
{
    public float FireSpeed;
    public float LifetimeDuration;
    public float Damage;
    public GameObject Prefab;
}