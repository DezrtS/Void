using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerController : NetworkBehaviour
{
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

    public event Action<bool> OnSelectionWheel;
    public PlayerLook PlayerLook => playerLook;
    public GameObject PlayerModel => playerModel;

    private void OnEnable()
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

    public virtual void EnableControls()
    {
        uiActionMap.Enable();
        playerActionMap.Enable();
    }

    public virtual void AssignControls()
    {
        uiActionMap ??= InputSystem.actions.FindActionMap("UI");
        uiActionMap.Enable();

        InputAction pauseActionInputAction = uiActionMap.FindAction("Pause");
        pauseActionInputAction.performed += OnPause;

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

    //private void OnDisable()
    //{
    //    UnassignControls();
    //}

    public virtual void DisableControls()
    {
        playerActionMap.Disable();
        uiActionMap.Disable();
    }

    public virtual void UnassignControls()
    {
        InputAction pauseActionInputAction = uiActionMap.FindAction("Pause");
        pauseActionInputAction.performed -= OnPause;

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
        if (!IsOwner) return;

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

    public abstract void OnSwitch(InputAction.CallbackContext context);

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (context.performed) playerLook.InteractWithObject();
    }

    public abstract void OnDrop(InputAction.CallbackContext context);
}