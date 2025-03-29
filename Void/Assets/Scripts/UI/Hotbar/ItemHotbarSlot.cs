using UnityEngine;

public class ItemHotbarSlot : HotbarSlot
{
    private Hotbar hotbar;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (hotbar)
        {
            hotbar.OnSwitchItem += OnSwitchItem;
            hotbar.OnPickUpItem += OnPickUpItem;
            hotbar.OnDropItem += OnDropItem;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (hotbar)
        {
            hotbar.OnSwitchItem -= OnSwitchItem;
            hotbar.OnPickUpItem -= OnPickUpItem;
            hotbar.OnDropItem -= OnDropItem;
        }
    }

    public override void OnSetupUI(GameObject player)
    {
        if (player.TryGetComponent(out Hotbar hotbar))
        {
            AttachHotbarSlot(hotbar);
        }
    }

    public void AttachHotbarSlot(Hotbar hotbar)
    {
        this.hotbar = hotbar;
        hotbar.OnSwitchItem += OnSwitchItem;
        hotbar.OnPickUpItem += OnPickUpItem;
        hotbar.OnDropItem += OnDropItem;

        if (hotbar.SelectedIndex == slotIndex) Activate();

        Item item = hotbar.GetItem(slotIndex);
        if (item != null)
        {
            UpdateHotbarImage(item.ItemData.ItemSprite);
            EnableDisableHotbarImage(true);
        }
    }

    public void OnSwitchItem(int fromIndex, int toIndex, Item fromItem, Item toItem)
    {
        OnSwitchSlot(fromIndex, toIndex);
    }

    public void OnPickUpItem(int index, Item item)
    {
        if (index != slotIndex) return;
        UpdateHotbarImage(item.ItemData.ItemSprite);
        EnableDisableHotbarImage(true);
    }

    public void OnDropItem(int index, Item item)
    {
        if (index != slotIndex) return;
        UpdateHotbarImage(null);
        EnableDisableHotbarImage(false);
    }
}