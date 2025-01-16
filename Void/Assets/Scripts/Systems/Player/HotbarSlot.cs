using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    [SerializeField] private int slotIndex;

    private Image hotbarImage;
    private Hotbar hotbar;

    private void OnEnable()
    {
        UIManager.OnSetupUI += OnSetupUI;

        if (hotbar)
        {
            hotbar.OnSwitchItem += OnSwitchItem;
            hotbar.OnPickUpItem += OnPickUpItem;
            hotbar.OnDropItem += OnDropItem;
        }
    }

    private void OnDisable()
    {
        UIManager.OnSetupUI -= OnSetupUI;

        if (hotbar)
        {
            hotbar.OnSwitchItem -= OnSwitchItem;
            hotbar.OnPickUpItem -= OnPickUpItem;
            hotbar.OnDropItem -= OnDropItem;
        }
    }

    private void Awake()
    {
        hotbarImage = GetComponent<Image>();
    }

    public void OnSetupUI(GameObject player)
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
    }

    public void DetachHotbarSlot()
    {
        hotbar.OnSwitchItem -= OnSwitchItem;
        hotbar.OnPickUpItem -= OnPickUpItem;
        hotbar.OnDropItem -= OnDropItem;
        hotbar = null;
    }

    public void OnSwitchItem(int fromIndex, int toIndex)
    {
        if (fromIndex == slotIndex)
        {
            // Deactivate
        } 
        else if (toIndex == slotIndex)
        {
            // Activate
        }
    }

    public void OnPickUpItem(int index, Item item)
    {
        if (index != slotIndex) return;
        UpdateHotbarImage(item.ItemData.ItemSprite);
    }

    public void OnDropItem(int index, Item item)
    {
        if (index != slotIndex) return;
        UpdateHotbarImage(null);
    }

    public void UpdateHotbarImage(Sprite sprite)
    {
        hotbarImage.sprite = sprite;
    }
}