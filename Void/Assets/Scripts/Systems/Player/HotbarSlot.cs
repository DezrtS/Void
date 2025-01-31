using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    [SerializeField] private int slotIndex;
    [SerializeField] private Image hotbarImage;
    [SerializeField] private float transitionDuration = 1;

    private Hotbar hotbar;
    private Animator animator;
    private bool active;
    private float activeValue;

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
        animator = GetComponent<Animator>();
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
            active = false;
            // Deactivate
        } 
        else if (toIndex == slotIndex)
        {
            active = true;
            // Activate
        }
    }

    private void FixedUpdate()
    {
        float fixedDeltaTime = (active) ? Time.fixedDeltaTime : -Time.fixedDeltaTime;
        activeValue = Mathf.Clamp(activeValue += fixedDeltaTime / transitionDuration, 0, 1);
        animator.SetFloat("Active", activeValue);
    }

    public void OnPickUpItem(int index, Item item)
    {
        if (index != slotIndex) return;
        UpdateHotbarImage(item.ItemData.ItemSprite);
        EnableDisableHotbarImage(true);
        active = true;
    }

    public void OnDropItem(int index, Item item)
    {
        if (index != slotIndex) return;
        UpdateHotbarImage(null);
        EnableDisableHotbarImage(false);
    }

    public void UpdateHotbarImage(Sprite sprite)
    {
        hotbarImage.sprite = sprite;
    }

    public void EnableDisableHotbarImage(bool enable)
    {
        hotbarImage.enabled = enable;
    }
}