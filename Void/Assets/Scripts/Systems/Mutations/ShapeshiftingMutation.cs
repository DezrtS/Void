using UnityEngine;

public class ShapeshiftingMutation : Mutation
{
    [SerializeField] private GameObject survivor;
    [SerializeField] private Animator animator;
    [SerializeField] private float duration;

    private NetworkShapeshiftingMutation networkShapeshiftingMutation;
    private bool isActive;

    AnimationController animationController;
    private GameObject playerModel;
    private float durationTimer;

    public bool CanActivate() => durationTimer <= 0;
    public bool CanDeactivate() => isActive;

    protected override void Awake()
    {
        base.Awake();
        networkShapeshiftingMutation = GetComponent<NetworkShapeshiftingMutation>();
    }

    public void RequestActivateShapeshiftingMutation() => networkShapeshiftingMutation.ActivateShapeshiftingMutationServerRpc();
    public void RequestDeactivateShapeshiftingMutation() => networkShapeshiftingMutation.DeactivateShapeshiftingMutationServerRpc();

    public override void SetupMutation(GameObject player)
    {
        base.SetupMutation(player);
        animationController = player.GetComponent<AnimationController>();
        playerModel = player.GetComponent<PlayerController>().PlayerModel;
    }

    public override void Use()
    {
        base.Use();
        if (networkShapeshiftingMutation.IsServer) RequestActivateShapeshiftingMutation();
    }

    public void Activate()
    {
        isActive = true;
        durationTimer = duration;
        playerModel.SetActive(false);
        survivor.SetActive(true);
        animationController.AddAnimatorInstance("Player", animator);
    }

    public override void UpdateTimers()
    {
        base.UpdateTimers();

        if (durationTimer > 0)
        {
            durationTimer -= Time.deltaTime;

            if (durationTimer <= 0)
            {
                durationTimer = 0;
                if (networkShapeshiftingMutation.IsServer) RequestDeactivateShapeshiftingMutation();
                cooldownTimer = mutationData.Cooldown;
            }
        }
    }

    public void Deactivate()
    {
        isActive = false;
        survivor.SetActive(false);
        playerModel.SetActive(true);
        animationController.RemoveAnimatorInstance(animator);
    }
}