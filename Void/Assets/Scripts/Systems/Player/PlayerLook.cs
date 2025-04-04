using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : NetworkBehaviour
{
    public delegate void InteractHandler(GameObject interactor, bool isInteracting);
    //public event InteractHandler OnInteract;

    [Header("Camera")]
    [SerializeField] private bool lockCameraOnCameraSpawn;

    [SerializeField] private bool spawnFirstPersonCamera;
    [SerializeField] private bool canLockCamera;
    [SerializeField] private bool lockPlayerCursor;
    [SerializeField] private Transform cameraRotationRootTransform;
    [SerializeField] private Transform cameraRootTransform;

    [SerializeField] private bool hasLookAtTarget;
    [SerializeField] private bool useOffsetLookAtTarget;
    [SerializeField] private Transform lookAtTransform;
    [SerializeField] private Transform lookAtTargetTransform;
    [SerializeField] private Transform offsetLookAtTargetTransform;

    [SerializeField] private float recoilMultiplier;

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

    private Transform target;

    private NetworkPlayerLook networkPlayerLook;
    private float xRotation;

    public event Action<bool> OnInteract;

    private InputActionMap cameraActionMap;
    private InputAction lookInputAction;

    private float currentXRotation = 0f;

    private IInteractable interactable;
    private UIManager instance;

    public NetworkPlayerLook NetworkPlayerLook => networkPlayerLook;
    public float XRotation { get { return xRotation; } set { xRotation = value; } }



    public Transform LookAtTargetTransform => lookAtTargetTransform;
    public bool CanLockCamera { get { return canLockCamera;  } set {  canLockCamera = value; } }
    public Transform CameraRotationRootTransform => cameraRotationRootTransform;
    public Transform CameraRootTransform => cameraRootTransform;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            AssignControls();
            if (spawnFirstPersonCamera)
            {
                GameObject camera = Instantiate(GameManager.Instance.FirstPersonCamera);
                CinemachineCamera cinemachineCamera = camera.GetComponentInChildren<CinemachineCamera>();
                cinemachineCamera.Follow = cameraRootTransform;
                cinemachineCamera.LookAt = lookAtTargetTransform;
            }
            if (lockCameraOnCameraSpawn) LockCamera(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsOwner)
        {
            UnassignControls();
        }
    }

    private void Start()
    {
        if (useOffsetLookAtTarget) target = offsetLookAtTargetTransform;
        else target = lookAtTargetTransform;
        instance = UIManager.Instance;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (hasLookAtTarget)
        {
            lookAtTransform.position = target.position;
        }

        Vector2 rotationInput = lookInputAction.ReadValue<Vector2>();

        float deltaYRotation = rotationInput.y * (invertY ? 1 : -1) * ySensitivity;
        currentXRotation += deltaYRotation;

        currentXRotation = Mathf.Clamp(currentXRotation, -maxYRotation, maxYRotation);

        cameraRotationRootTransform.localRotation = Quaternion.Euler(currentXRotation, 0, 0);
        SetXRotationServerRpc(currentXRotation);

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
                if (hitInfo.collider.TryGetComponent(out IInteractable interactable))
                {
                    InteractableData interactableData = interactable.GetInteractableData();
                    instance.SetInteractableText(interactableData);
                }
                else if (this.interactable != null)
                {
                    instance.ResetInteractableText();
                }

                this.interactable = interactable;
            }
            else
            {
                if (interactable != null)
                {
                    instance.ResetInteractableText();
                }
                interactable = null;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetXRotationServerRpc(float xRotation, ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(serverRpcParams);
        SetXRotationClientRpc(xRotation, clientRpcParams);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SetXRotationClientRpc(float xRotation, ClientRpcParams clientRpcParams = default)
    {
        currentXRotation = xRotation;
        cameraRotationRootTransform.localRotation = Quaternion.Euler(currentXRotation, 0, 0);
    }


    public void AssignControls()
    {
        if (!IsOwner) return;

        cameraActionMap ??= InputSystem.actions.FindActionMap("Camera");
        cameraActionMap.Enable();

        lookInputAction = cameraActionMap.FindAction("Look");
        UIManager.OnPause += LockCamera;
    }

    public void UnassignControls()
    {
        if (!IsOwner) return;

        cameraActionMap ??= InputSystem.actions.FindActionMap("Camera");
        cameraActionMap.Disable();

        lookInputAction = null;
        UIManager.OnPause -= LockCamera;
    }

    public void SpawnFirstPersonCamera()
    {
        GameObject camera = Instantiate(GameManager.Instance.FirstPersonCamera);
        CinemachineCamera cinemachineCamera = camera.GetComponentInChildren<CinemachineCamera>();
        cinemachineCamera.Follow = cameraRootTransform;
        cinemachineCamera.LookAt = lookAtTargetTransform;
    }

    public void AddXRotation(float value)
    {
        if (!IsOwner) return;

        currentXRotation = Mathf.Clamp(currentXRotation + value * recoilMultiplier, -maxYRotation, maxYRotation);
    }

    public void AddRandomYRotation()
    {
        if (!IsOwner) return;

        transform.Rotate(new Vector3(0, (UnityEngine.Random.Range(-100, 100) / 100f) * recoilMultiplier, 0));
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

    public void LockCamera(bool isLocked)
    {
        if (isLocked)
        {
            cameraActionMap.Disable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Locked Camera (Can't Move)");
        }
        else
        {
            if (!canLockCamera) return;
            cameraActionMap.Enable();

            if (!lockPlayerCursor) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Unlocked Camera (Can Move)");
        }
    }

    public void InteractWithObject()
    {
        interactable?.Interact(gameObject);
        OnInteract?.Invoke(true);
    }

    public void UninteractWithObject()
    {
        OnInteract?.Invoke(false);
    }
}