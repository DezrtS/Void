using FMODUnity;
using UnityEngine;

public class ShapeshiftingMutation : Mutation
{
    [SerializeField] private GameObject survivor;
    [SerializeField] private Transform lookAtTransform;
    [SerializeField] private GameObject[] survivorModelVisuals;
    [SerializeField] private GameObject[] disableForOwner;
    [SerializeField] private Animator animator;
    [SerializeField] private float duration;

    [SerializeField] private EventReference shapeshiftBackSound;

    private NetworkShapeshiftingMutation networkShapeshiftingMutation;
    private bool isActive;

    private PlayerController playerController;
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
        if (networkShapeshiftingMutation.IsOwner)
        {
            foreach (GameObject gameObject in disableForOwner)
            {
                gameObject.layer = LayerMask.NameToLayer("Ignore Local");
            }
        }
        playerController = player.GetComponent<PlayerController>();
        animationController.AddAnimatorInstance("Player", animator);
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
        cooldownTimer = mutationData.Cooldown;
        playerController.HidePlayerModel(true);
        HideModel(false);
        //animationController.AddAnimatorInstance("Player", animator);
    }

    public override void UpdateTimers()
    {
        base.UpdateTimers();

        if (durationTimer > 0)
        {
            durationTimer -= Time.deltaTime;
            lookAtTransform.position = playerController.PlayerLook.LookAtTargetTransform.position;
            if (durationTimer <= 0)
            {
                durationTimer = 0;
                if (networkShapeshiftingMutation.IsServer) RequestDeactivateShapeshiftingMutation();
            }
        }
    }

    public void Deactivate()
    {
        isActive = false;
        HideModel(true);
        playerController.HidePlayerModel(false);
        //animationController.RemoveAnimatorInstance(animator);
        AudioManager.PlayOneShot(shapeshiftBackSound, gameObject);
    }

    public void HideModel(bool hide)
    {
        foreach (GameObject gameObject in survivorModelVisuals)
        {
            gameObject.SetActive(!hide);
        }
    }
}