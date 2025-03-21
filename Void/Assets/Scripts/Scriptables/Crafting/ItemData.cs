using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/Items/Data/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite ItemSprite;
    public GameObject ItemPrefab;

    [SerializeField] private EventReference useSound;
    [SerializeField] private EventReference stopUsingSound;
    [SerializeField] private EventReference pickUpSound;
    [SerializeField] private EventReference dropSound;

    public EventReference UseSound => useSound;
    public EventReference StopUsingSound => stopUsingSound;
    public EventReference PickUpSound => pickUpSound;
    public EventReference DropSound => dropSound;
}