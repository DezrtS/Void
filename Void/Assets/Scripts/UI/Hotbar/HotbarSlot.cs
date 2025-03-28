using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class HotbarSlot : MonoBehaviour
{
    [SerializeField] protected int slotIndex;
    [SerializeField] private Image hotbarImage;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color cooldownColor;
    [SerializeField] private TextMeshProUGUI slotIndexText;
    [SerializeField] private float transitionDuration = 1;

    private Animator animator;
    private bool active;
    private float activeValue;

    private float cooldownTime;
    private float cooldownTimer;

    protected virtual void OnEnable()
    {
        UIManager.OnSetupUI += OnSetupUI;
    }

    protected virtual void OnDisable()
    {
        UIManager.OnSetupUI -= OnSetupUI;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        slotIndexText.text = $"{slotIndex + 1}";
    }

    private void FixedUpdate()
    {
        float fixedDeltaTime = Time.fixedDeltaTime;
        if (cooldownTimer > 0)
        {
            cooldownTimer -= fixedDeltaTime;
            if (cooldownTimer <= 0)
            {
                cooldownTimer = 0;
                hotbarImage.fillAmount = 1;
                hotbarImage.color = defaultColor;
            }
            else
            {
                float percentage = cooldownTimer / cooldownTime;
                hotbarImage.fillAmount = 1 - percentage;
            }
        }

        if (!active) fixedDeltaTime *= -1;
        activeValue = Mathf.Clamp(activeValue += fixedDeltaTime / transitionDuration, 0, 1);
        animator.SetFloat("Active", activeValue);
    }

    public abstract void OnSetupUI(GameObject player);

    public void OnSwitchSlot(int fromIndex, int toIndex)
    {
        if (fromIndex == slotIndex)
        {
            Deactivate();
        } 
        else if (toIndex == slotIndex)
        {
            Activate();
        }
    }

    public virtual void Activate()
    {
        active = true;
    }

    public virtual void Deactivate()
    {
        active = false;
    }

    public void SetCooldownTimer(float time)
    {
        cooldownTime = time;
        cooldownTimer = time;
        hotbarImage.fillAmount = 0;
        hotbarImage.color = cooldownColor;
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