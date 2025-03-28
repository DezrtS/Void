using UnityEngine;

public class Revive : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractableData reviveInteractableData;
    [SerializeField] private InteractableData revivingInteractableData;

    [SerializeField] private float reviveTime;

    private Health health;
    private bool isReviving;
    private float reviveTimer;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void FixedUpdate()
    {
        if (reviveTimer > 0)
        {
            reviveTimer -= Time.fixedDeltaTime;
            if (reviveTimer <= 0)
            {
                isReviving = false;
                health.RequestRespawn();
            }
        }
    }

    public InteractableData GetInteractableData()
    {
        if (!health.IsDead) return null;

        if (isReviving) return revivingInteractableData;
        return reviveInteractableData;
    }

    public void Interact(GameObject interactor)
    {
        if (isReviving || !health.IsDead) return;

        isReviving = true;
        reviveTimer = reviveTime;
    }
}