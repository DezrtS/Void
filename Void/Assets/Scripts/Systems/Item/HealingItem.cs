using UnityEngine;

public class HealingItem : Item
{
    private HealingItemData healingItemData;
    private Health health;

    private void Start()
    {
        healingItemData = ItemData as HealingItemData;
    }

    public override void Use()
    {
        base.Use();
        Activate();
    }

    public void Activate()
    {
        health = GetComponentInParent<Health>();
        if (health == null) return;
        if (NetworkItem.IsServer)
        {
            health.RequestHealthChangeOverTime(healingItemData.Amount, healingItemData.Duration);
            RequestDrop();
        }
    }
}
