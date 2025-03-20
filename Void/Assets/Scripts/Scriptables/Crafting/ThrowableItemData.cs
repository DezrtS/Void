using FMOD.Studio;
using UnityEngine;

[CreateAssetMenu(fileName = "ThrowableItemData", menuName = "ScriptableObjects/Items/Data/ThrowableItemData")]
public class ThrowableItemData : ItemData
{
    [SerializeField] private float throwSpeed;
    [SerializeField] private float spinSpeed;
    [SerializeField] private EventInstance throwSound;

    public float ThrowSpeed => throwSpeed;
    public float SpinSpeed => spinSpeed;
    public EventInstance ThrowSound => throwSound;
}