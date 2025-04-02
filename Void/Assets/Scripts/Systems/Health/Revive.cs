using UnityEngine;

public class Revive : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractableData reviveInteractableData;
    [SerializeField] private InteractableData revivingInteractableData;

    [SerializeField] private float reviveTime;

    private PlayerLook playerLook;
    private MovementController movementController;
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
                OnInteract(false);
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

        interactor.TryGetComponent(out playerLook);
        interactor.TryGetComponent(out movementController);
        playerLook.OnInteract += OnInteract;
        playerLook.EnableDisableCameraControls(false);
        movementController.RequestSetInputDisabled(true);
        isReviving = true;
        reviveTimer = reviveTime;
        UIManager.Instance.SetCooldownTimer(reviveTimer);
    }

    public void OnInteract(bool isInteracting)
    {
        if (!isInteracting)
        {
            playerLook.OnInteract -= OnInteract;
            playerLook.EnableDisableCameraControls(true);
            movementController.RequestSetInputDisabled(false);
            isReviving = false;
            reviveTimer = 0;
            UIManager.Instance.ResetCooldownTimer();
            playerLook = null;
            movementController = null;
        }
    }
}