using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerController : NetworkBehaviour
{
    public event Action<bool> OnSelectionWheel;

    [SerializeField] private GameManager.PlayerRole playerRole;
    [SerializeField] private bool canAutomaticallyRespawn;
    [SerializeField] private float respawnDelay;

    [SerializeField] private GameObject playerModel;
    [SerializeField] private bool disableForOwnerOnStart;
    [SerializeField] private GameObject[] disableForOwner;

    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;

    protected MovementController movementController;
    protected PlayerLook playerLook;
    protected Health health;

    private float respawnTimer;
    public GameManager.PlayerRole PlayerRole => playerRole;
    public PlayerLook PlayerLook => playerLook;
    public GameObject PlayerModel => playerModel;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            AssignControls();

            UIManager.OnPause += (bool paused) =>
            {
                if (paused)
                {
                    playerActionMap.Disable();
                }
                else
                {
                    playerActionMap.Enable();
                }
            };
        }
    }

    private void OnDisable()
    {
        UnassignControls();
    }

    public virtual void EnableControls()
    {
        if (!IsOwner) return;

        uiActionMap.Enable();
        playerActionMap.Enable();
    }

    public virtual void AssignControls()
    {
        if (!IsOwner) return;

        uiActionMap ??= InputSystem.actions.FindActionMap("UI");
        uiActionMap.Enable();

        InputAction pauseActionInputAction = uiActionMap.FindAction("Pause");
        pauseActionInputAction.performed += OnPause;

        InputAction hideTutorialInputAction = uiActionMap.FindAction("Hide Tutorial");
        hideTutorialInputAction.performed += OnHideTutorial;

        playerActionMap ??= InputSystem.actions.FindActionMap("Player");
        playerActionMap.Enable();

        InputAction primaryActionInputAction = playerActionMap.FindAction("Primary Action");
        primaryActionInputAction.performed += OnPrimaryAction;
        primaryActionInputAction.canceled += OnPrimaryAction;

        InputAction secondaryActionInputAction = playerActionMap.FindAction("Secondary Action");
        secondaryActionInputAction.performed += OnSecondaryAction;
        secondaryActionInputAction.canceled += OnSecondaryAction;

        InputAction openSelectionInputAction = playerActionMap.FindAction("Open Selection");
        openSelectionInputAction.performed += OnOpenSelection;
        openSelectionInputAction.canceled += OnOpenSelection;

        InputAction switchInputAction = playerActionMap.FindAction("Switch");
        switchInputAction.performed += OnSwitch;

        InputAction interactInputAction = playerActionMap.FindAction("Interact");
        interactInputAction.performed += OnInteract;

        InputAction dropInputAction = playerActionMap.FindAction("Drop");
        dropInputAction.performed += OnDrop;

        EnableControls();
    }

    public virtual void DisableControls()
    {
        if (!IsOwner) return;

        playerActionMap.Disable();
        uiActionMap.Disable();
    }

    public virtual void UnassignControls()
    {
        if (!IsOwner) return;

        InputAction pauseActionInputAction = uiActionMap.FindAction("Pause");
        pauseActionInputAction.performed -= OnPause;

        InputAction hideTutorialInputAction = uiActionMap.FindAction("Hide Tutorial");
        hideTutorialInputAction.performed -= OnHideTutorial;

        InputAction primaryActionInputAction = playerActionMap.FindAction("Primary Action");
        primaryActionInputAction.performed -= OnPrimaryAction;
        primaryActionInputAction.canceled -= OnPrimaryAction;

        InputAction secondaryActionInputAction = playerActionMap.FindAction("Secondary Action");
        secondaryActionInputAction.performed -= OnSecondaryAction;
        secondaryActionInputAction.canceled -= OnSecondaryAction;

        InputAction openSelectionInputAction = playerActionMap.FindAction("Open Selection");
        openSelectionInputAction.performed -= OnOpenSelection;
        openSelectionInputAction.canceled -= OnOpenSelection;

        InputAction switchInputAction = playerActionMap.FindAction("Switch");
        switchInputAction.performed -= OnSwitch;

        InputAction interactInputAction = playerActionMap.FindAction("Interact");
        interactInputAction.performed -= OnInteract;

        InputAction dropInputAction = playerActionMap.FindAction("Drop");
        dropInputAction.performed -= OnDrop;

        DisableControls();
    }

    protected virtual void Awake()
    {
        movementController = GetComponent<MovementController>();
        playerLook = GetComponent<PlayerLook>();
        health = GetComponent<Health>();
        health.OnDeathStateChanged += OnDeathStateChanged;
        //GameManager.OnGameStateChanged += (GameManager.GameState gameState) =>
        //{
        //    if (gameState == GameManager.GameState.GameOver)
        //    {
        //        UnassignControls();
        //        if (IsServer) NetworkObject.Despawn();
        //    }
        //};
    }

    private void Start()
    {
        if (!IsOwner || !disableForOwnerOnStart) return;
        foreach (GameObject gameObject in disableForOwner)
        {
            gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        UpdateTimers(Time.fixedDeltaTime);
    }

    public virtual void UpdateTimers(float deltaTime)
    {
        if (respawnTimer > 0)
        {
            respawnTimer -= deltaTime;
            if (respawnTimer < 0)
            {
                health.RequestRespawn();
            }
        }
    }

    public virtual void OnDeathStateChanged(Health health, bool isDead)
    {
        if (!IsOwner) return;

        movementController.RequestSetInputDisabled(isDead);
        if (isDead)
        {
            movementController.SetVelocity(Vector3.zero);
            if (canAutomaticallyRespawn) respawnTimer = respawnDelay;
        }
        else
        {
            movementController.SetRotation(Quaternion.identity);
        }
    }

    public abstract void OnPrimaryAction(InputAction.CallbackContext context);
    public abstract void OnSecondaryAction(InputAction.CallbackContext context);
    public void OnOpenSelection(InputAction.CallbackContext context)
    {
        if (!IsOwner || health.IsDead) return;

        if (context.performed)
        {
            OnSelectionWheel?.Invoke(true);
            return;
        }

        if (context.canceled)
        {
            OnSelectionWheel?.Invoke(false);
            return;
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        UIManager.Instance.PauseUnpauseGame();
    }

    public void OnHideTutorial(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        UIManager.Instance.HideUnhideTutorialUI();
    }

    public abstract void OnSwitch(InputAction.CallbackContext context);

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!IsOwner || health.IsDead) return;
        if (context.performed) playerLook.InteractWithObject();
    }

    public abstract void OnDrop(InputAction.CallbackContext context);
}