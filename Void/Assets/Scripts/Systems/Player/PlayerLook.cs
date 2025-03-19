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
    [SerializeField] private Transform cameraRotationRootTransform;
    [SerializeField] private Transform cameraRootTransform;

    [SerializeField] private bool hasLookAtTarget;
    [SerializeField] private Transform lookAtTransform;
    [SerializeField] private Transform lookAtTargetTransform;

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
    private UIManager instance;

    private void OnEnable()
    {
        cameraActionMap ??= InputSystem.actions.FindActionMap("Camera");
        cameraActionMap.Enable();

        lookInputAction = cameraActionMap.FindAction("Look");

        UIManager.OnPause += (bool paused) =>
        {
            if (paused)
            {
                LockCamera(false);
            }
            else
            {
                LockCamera(true);
            }
        };
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
            CinemachineCamera cinemachineCamera = camera.GetComponentInChildren<CinemachineCamera>();
            cinemachineCamera.Follow = cameraRootTransform;
            cinemachineCamera.LookAt = lookAtTargetTransform;
        }

        LockCamera(true);
    }

    private void Start()
    {
        instance = UIManager.Instance;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (hasLookAtTarget)
        {
            lookAtTransform.position = lookAtTargetTransform.position;
        }

        Vector2 rotationInput = lookInputAction.ReadValue<Vector2>();

        float deltaYRotation = rotationInput.y * (invertY ? 1 : -1) * ySensitivity;
        currentXRotation += deltaYRotation;

        currentXRotation = Mathf.Clamp(currentXRotation, -maxYRotation, maxYRotation);

        cameraRotationRootTransform.localRotation = Quaternion.Euler(currentXRotation, 0, 0);

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
            //Debug.DrawRay(cameraRootTransform.position, cameraRootTransform.forward, Color.red, 1);
            if (Physics.Raycast(cameraRootTransform.position, (lookAtTargetTransform.position - cameraRootTransform.position).normalized, out RaycastHit hitInfo, interactRange, interactLayerMask, QueryTriggerInteraction.Collide))
            {
                hitInfo.collider.TryGetComponent(out IInteractable interactable);

                //if (this.interactable != interactable)
                //{
                    InteractableData interactableData = interactable.GetInteractableData();
                    instance.SetInteractableText(interactableData);
                //}

                this.interactable = interactable;
            }
            else
            {
                if (interactable != null)
                {
                    UIManager.Instance.ResetInteractableText();
                }
                interactable = null;
            }
        }
    }

    public void AddXRotation(float value)
    {
        if (!IsOwner) return;

        currentXRotation = Mathf.Clamp(currentXRotation + value, -maxYRotation, maxYRotation);
    }

    public void AddRandomYRotation()
    {
        if (!IsOwner) return;

        transform.Rotate(new Vector3(0, Random.Range(-100, 100) / 100f, 0));
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
            cameraActionMap.Enable();

            if (!lockPlayerCursor) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            cameraActionMap.Disable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void InteractWithObject()
    {
        interactable?.Interact(gameObject);
    }
}