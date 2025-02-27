using UnityEngine;

public class ShapeshiftingMutation : Mutation
{
    [SerializeField] private GameObject survivor;
    [SerializeField] private Animator animator;
    [SerializeField] private float duration;

    AnimationController animationController;
    private GameObject playerModel;
    private float durationTimer;

    public bool CanActivate() => durationTimer <= 0;

    public override void SetupMutation(GameObject player)
    {
        base.SetupMutation(player);
        animationController = player.GetComponent<AnimationController>();
        playerModel = player.GetComponent<PlayerController>().PlayerModel;
    }

    public override void Use()
    {
        base.Use();
        if (CanActivate()) Activate();
    }

    public void Activate()
    {
        durationTimer = duration;
        //if (networkUseable.IsOwner) return;
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
                Deactivate();
                cooldownTimer = mutationData.Cooldown;
            }
        }
    }

    public void Deactivate()
    {
        //if (networkUseable.IsOwner) return;
        survivor.SetActive(false);
        playerModel.SetActive(true);
        animationController.RemoveAnimatorInstance(animator);
    }
}