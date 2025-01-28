using UnityEngine;

public abstract class MutationData : ScriptableObject
{
    [SerializeField] private string displayName;
    [TextArea(2, 5)]
    [SerializeField] private string description;
    [SerializeField] private Sprite displaySprite;
    [SerializeField] private GameObject mutationPrefab;
    [SerializeField] private float cooldown;

    public string DisplayName => displayName;
    public string Description => description;
    public Sprite DisplaySprite => displaySprite;
    public GameObject MutationPrefab => mutationPrefab;
    public float Cooldown => cooldown;
}