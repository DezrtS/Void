using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "ScriptableObjects/Projectiles/ProjectileData", order = 1)]
public class ProjectileData : ScriptableObject
{
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private float fireSpeed;
    [SerializeField] private Vector3 angularVelocity;
    [SerializeField] private float gravity;

    [SerializeField] private bool useBoxCast = true;
    [SerializeField] private Vector2 boxCastSize = Vector2.one;

    [SerializeField] private float lifetimeDuration;
    [SerializeField] private float damage;
    [SerializeField] private float knockback;

    [SerializeField] private GameObject hitEffect;
    [SerializeField] private EventReference spawnSound;
    [SerializeField] private EventReference hitSound;
    [SerializeField] private LayerMask layerMask;

    public GameObject ProjectilePrefab => projectilePrefab;
    public float FireSpeed => fireSpeed;
    public Vector3 AngularVelocity => angularVelocity;
    public float Gravity => gravity;
    public bool UseBoxCast => useBoxCast;
    public Vector2 BoxCastSize => boxCastSize;
    public float LifetimeDuration => lifetimeDuration;
    public float Damage => damage;
    public float Knockback => knockback;
    public GameObject HitEffect => hitEffect;
    public EventReference SpawnSound => spawnSound;
    public EventReference HitSound => hitSound;
    public LayerMask LayerMask => layerMask;
}