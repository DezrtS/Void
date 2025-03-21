using UnityEngine;
using UnityEngine.UI;

public class MutationHotbarSlot : MonoBehaviour
{
    [SerializeField] private int slotIndex;
    [SerializeField] private Image hotbarImage;
    [SerializeField] private float transitionDuration = 1;

    private MutationHotbar mutationHotbar;
    private Animator animator;
    private bool active;
    private float activeValue;

    private void OnEnable()
    {
        UIManager.OnSetupUI += OnSetupUI;

        if (mutationHotbar)
        {
            mutationHotbar.OnSwitchMutation += OnSwitchMutation;
            mutationHotbar.OnAddMutation += OnAddMutation;
            mutationHotbar.OnRemoveMutation += OnRemoveMutation;
        }
    }

    private void OnDisable()
    {
        UIManager.OnSetupUI -= OnSetupUI;

        if (mutationHotbar)
        {
            mutationHotbar.OnSwitchMutation -= OnSwitchMutation;
            mutationHotbar.OnAddMutation -= OnAddMutation;
            mutationHotbar.OnRemoveMutation -= OnRemoveMutation;
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnSetupUI(GameObject player)
    {
        if (player.TryGetComponent(out MutationHotbar mutationHotbar))
        {
            AttachMutationHotbarSlot(mutationHotbar);
        }
    }

    public void AttachMutationHotbarSlot(MutationHotbar mutationHotbar)
    {
        this.mutationHotbar = mutationHotbar;
        mutationHotbar.OnSwitchMutation += OnSwitchMutation;
        mutationHotbar.OnAddMutation += OnAddMutation;
        mutationHotbar.OnRemoveMutation += OnRemoveMutation;

        if (mutationHotbar.SelectedIndex == slotIndex) active = true;

        Mutation mutation = mutationHotbar.GetMutation(slotIndex);
        if (mutation != null)
        {
            UpdateHotbarImage(mutation.MutationData.DisplaySprite);
            EnableDisableHotbarImage(true);
        }
    }

    public void DetachHotbarSlot()
    {
        mutationHotbar.OnSwitchMutation -= OnSwitchMutation;
        mutationHotbar.OnAddMutation -= OnAddMutation;
        mutationHotbar.OnRemoveMutation -= OnRemoveMutation;
        mutationHotbar = null;
    }

    public void OnSwitchMutation(int fromIndex, int toIndex)
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

    public void OnAddMutation(Mutation mutation, int index)
    {
        if (index != slotIndex) return;
        UpdateHotbarImage(mutation.MutationData.DisplaySprite);
        EnableDisableHotbarImage(true);
        if (index == 0) active = true;
    }

    public void OnRemoveMutation(Mutation mutation, int index)
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
