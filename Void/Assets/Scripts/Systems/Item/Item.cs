using UnityEngine;

public abstract class Item : MonoBehaviour, IUseable
{
    [SerializeField] private ItemData ItemData;
    [SerializeField] protected bool canUse = true;
    [SerializeField] protected bool canPickUp = true;
    [SerializeField] protected bool canDrop = true;

    public bool CanUse { get { return canUse; } }
    public bool CanPickUp { get { return canPickUp; } }
    public bool CanDrop { get { return canDrop; } }

    public abstract void Use();
    public virtual void OnPickUp()
    {
        canPickUp = false;
        canDrop = true;
    }

    public virtual void OnDrop()
    {
        canDrop = false;
        canPickUp = true;
    }
}