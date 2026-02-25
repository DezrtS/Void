using UnityEngine;

public abstract class Mutation : MonoBehaviour, INetworkUseable
{
    public event IUseable.UseHandler OnUsed;

    [SerializeField] protected MutationData mutationData;
    [SerializeField] private TutorialData tutorialData;
    [SerializeField] private bool disableActiveTransformOverride;

    protected NetworkUseable networkUseable;
    private bool isUsing;

    protected float cooldownTimer;
    protected GameObject player;
    protected AnimationController animationController;

    public MutationData MutationData => mutationData;
    public TutorialData TutorialData => tutorialData;
    public bool DisableActiveTransformOverride => disableActiveTransformOverride;
    public NetworkUseable NetworkUseable => networkUseable;
    public bool IsUsing => isUsing;

    public bool CanUse() => !isUsing && cooldownTimer <= 0;
    public bool CanStopUsing() => isUsing;

    public void RequestUse() => networkUseable.UseServerRpc();
    public void RequestStopUsing() => networkUseable.StopUsingServerRpc();

    protected virtual void Awake()
    {
        networkUseable = GetComponent<NetworkUseable>();
    }

    private void FixedUpdate()
    {
        UpdateTimers();
    }

    public virtual void SetupMutation(GameObject player)
    {
        this.player = player;
        player.TryGetComponent(out animationController);
    }

    public virtual void Use()
    {
        //Debug.Log("USED MUTATION");
        isUsing = true;
        OnUsed?.Invoke(this, isUsing);
        AudioManager.PlayOneShot(mutationData.UseSound, gameObject);
    }

    public virtual void StopUsing()
    {
        isUsing = false;
        OnUsed?.Invoke(this, isUsing);
        AudioManager.PlayOneShot(mutationData.StopUsingSound, gameObject);
    }

    public virtual void UpdateTimers()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.fixedDeltaTime;

            if (cooldownTimer <= 0)
            {
                cooldownTimer = 0;
            }
        }
    }
}