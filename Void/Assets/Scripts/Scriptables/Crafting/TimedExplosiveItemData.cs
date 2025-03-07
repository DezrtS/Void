using FMOD.Studio;
using UnityEngine;

[CreateAssetMenu(fileName = "TimedExplosiveItemData", menuName = "ScriptableObjects/Items/Data/TimedExplosiveItemData")]
public class TimedExplosiveItemData : ThrowableItemData
{
    [SerializeField] private float radius;
    [SerializeField] private float damage;
    [SerializeField] private bool explodeOnImpact;
    [SerializeField] private float timeToActivation;
    [SerializeField] private LayerMask effectableLayers;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private EventInstance explosionSound;

    public float Radius => radius;
    public float Damage => damage;
    public bool ExplodeOnImpact => explodeOnImpact;
    public float TimeToActivation => timeToActivation;
    public LayerMask EffectableLayers => effectableLayers;
    public GameObject ExplosionEffect => explosionEffect;
    public EventInstance ExplosionSound => explosionSound;
}