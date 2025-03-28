using UnityEngine;

public class MutationHotbarSlot : HotbarSlot
{
    private MutationHotbar mutationHotbar;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (mutationHotbar)
        {
            mutationHotbar.OnSwitchMutation += OnSwitchSlot;
            mutationHotbar.OnAddMutation += OnAddMutation;
            mutationHotbar.OnRemoveMutation += OnRemoveMutation;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (mutationHotbar)
        {
            mutationHotbar.OnSwitchMutation -= OnSwitchSlot;
            mutationHotbar.OnAddMutation -= OnAddMutation;
            mutationHotbar.OnRemoveMutation -= OnRemoveMutation;
        }
    }

    public override void OnSetupUI(GameObject player)
    {
        if (player.TryGetComponent(out MutationHotbar mutationHotbar))
        {
            AttachHotbarSlot(mutationHotbar);
        }
    }

    public void AttachHotbarSlot(MutationHotbar mutationHotbar)
    {
        this.mutationHotbar = mutationHotbar;
        mutationHotbar.OnSwitchMutation += OnSwitchSlot;
        mutationHotbar.OnAddMutation += OnAddMutation;
        mutationHotbar.OnRemoveMutation += OnRemoveMutation;

        if (mutationHotbar.SelectedIndex == slotIndex) Activate();

        Mutation mutation = mutationHotbar.GetMutation(slotIndex);
        if (mutation != null)
        {
            mutation.OnUsed += OnUseMutation;
            UpdateHotbarImage(mutation.MutationData.DisplaySprite);
            EnableDisableHotbarImage(true);
        }
    }

    public void OnUseMutation(IUseable useable, bool isUsing)
    {
        Mutation mutation = useable as Mutation;
        SetCooldownTimer(mutation.MutationData.Cooldown);
    }

    public void OnAddMutation(Mutation mutation, int index)
    {
        if (index != slotIndex) return;
        mutation.OnUsed += OnUseMutation;
        UpdateHotbarImage(mutation.MutationData.DisplaySprite);
        EnableDisableHotbarImage(true);
    }

    public void OnRemoveMutation(Mutation mutation, int index)
    {
        if (index != slotIndex) return;
        mutation.OnUsed -= OnUseMutation;
        UpdateHotbarImage(null);
        EnableDisableHotbarImage(false);
    }
}
