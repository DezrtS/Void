using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "MutationData", menuName = "ScriptableObjects/Mutations/MutationData")]
public class MutationData : ScriptableObject
{
    [SerializeField] private string displayName;
    [TextArea(2, 5)]
    [SerializeField] private string description;
    [SerializeField] private Sprite displaySprite;
    [SerializeField] private GameObject mutationPrefab;
    [SerializeField] private float cooldown;

    [SerializeField] private EventReference useSound;
    [SerializeField] private EventReference stopUsingSound;

    public string DisplayName => displayName;
    public string Description => description;
    public Sprite DisplaySprite => displaySprite;
    public GameObject MutationPrefab => mutationPrefab;
    public float Cooldown => cooldown;

    public EventReference UseSound => useSound;
    public EventReference StopUsingSound => stopUsingSound;
}