using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : NetworkBehaviour
{
    private InputActionMap cameraActionMap;
    private InputAction lookInputAction;

    private float currentXRotation = 0f;

    [Header("Camera")]
    [SerializeField] private bool spawnFirstPersonCamera;
    [SerializeField] private bool lockPlayerCursor;
    [SerializeField] private Transform cameraRootTransform;

    [Header("Interaction")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private float interactRange = 5;
    [SerializeField] private LayerMask interactLayerMask;

    [Header("Y-Axis")]
    [SerializeField] private float maxYRotation = 90f;
    [SerializeField] private bool invertY = false;

    [Header("Sensitivity")]
    [SerializeField] private float xSensitivity = 0.02f;
    [SerializeField] private float ySensitivity = 0.01f;

    private IInteractable interactable;

    private void OnEnable()
    {
        cameraActionMap ??= InputSystem.actions.FindActionMap("Camera");
        cameraActionMap.Enable();

        lookInputAction = cameraActionMap.FindAction("Look");
    }

    private void OnDisable()
    {
        cameraActionMap.Disable();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (spawnFirstPersonCamera && IsOwner)
        {
            GameObject camera = Instantiate(GameManager.Instance.FirstPersonCamera);
            camera.GetComponentInChildren<CinemachineCamera>().Follow = cameraRootTransform;
        }
    }

    private void Awake()
    {
        LockCamera(true);
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 rotationInput = lookInputAction.ReadValue<Vector2>();

        float deltaYRotation = rotationInput.y * (invertY ? 1 : -1) * ySensitivity;
        currentXRotation += deltaYRotation;

        currentXRotation = Mathf.Clamp(currentXRotation, -maxYRotation, maxYRotation);

        cameraRootTransform.localRotation = Quaternion.Euler(currentXRotation, 0, 0);

        Quaternion newRotation = Quaternion.Euler(0, transform.eulerAngles.y + rotationInput.x * xSensitivity, 0);

        if (transform.rotation != newRotation)
        {
            transform.rotation = newRotation;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (canInteract)
        {
            Debug.DrawRay(cameraRootTransform.position, cameraRootTransform.forward, Color.red, 1);
            if (Physics.Raycast(cameraRootTransform.position, cameraRootTransform.forward, out RaycastHit hitInfo, interactRange, interactLayerMask, QueryTriggerInteraction.Collide))
            {
                hitInfo.collider.TryGetComponent(out IInteractable interactable);
                this.interactable = interactable;
            }
            else
            {
                interactable = null;
            }
        }
    }

    public void EnableDisableCameraControls(bool enabled)
    {
        if (enabled)
        {
            cameraActionMap.Enable();
        }
        else
        {
            cameraActionMap.Disable();
        }
    }

    public void LockCamera(bool locked)
    {
        if (locked)
        {
            if (!lockPlayerCursor) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void InteractWithObject()
    {
        interactable?.Interact(gameObject);
    }
}