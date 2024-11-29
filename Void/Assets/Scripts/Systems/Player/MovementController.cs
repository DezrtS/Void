using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class MovementController : NetworkBehaviour
{
    public Transform spawnPoint;
    public GameObject FPSCameraPrefab;

    private Camera playerCamera;
    private CharacterController characterController;

    public GameObject lookAt;

    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Movement settings
    private float walkSpeed = 3f;
    private float runSpeed = 6f;
    private float jumpPower = 7f;
    private float gravity = 13f;
    private float lookSpeed = 2f;
    private float lookXLimit = 45f;
    private float defaultHeight = 2f;
    private float crouchHeight = 1f;
    private float crouchSpeed = 1.5f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    private Vector3 velocity;

    private bool canMove = true;

    private DamageSystem damageSystem;

    [SerializeField] private Animator animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            SpawnPlayerCamera();
        }

        damageSystem = GetComponent<DamageSystem>();

        if (damageSystem != null)
        {
            damageSystem.isDead.OnValueChanged += (oldValue, newValue) =>
            {
                if (newValue)
                {
                    Die();
                }
            };
        }
        else
        {
            Debug.LogWarning("DamageSystem not found on player.");
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner || isDead.Value)
        {
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!canMove) return;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        animator.SetFloat("xinput", curSpeedY / (isRunning ? runSpeed : walkSpeed));
        animator.SetFloat("yinput", curSpeedX / (isRunning ? runSpeed : walkSpeed));

        characterController.Move(moveDirection * Time.deltaTime);

        if (playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            lookAt.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.Rotate(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    private void SpawnPlayerCamera()
    {
        if (FPSCameraPrefab == null || spawnPoint == null)
        {
            Debug.LogError("FPSCameraPrefab or spawnPoint is not assigned in the Inspector.");
            return;
        }

        GameObject playerCameraObject = Instantiate(FPSCameraPrefab, spawnPoint.position, spawnPoint.rotation);
        CameraController cameraController = playerCameraObject.GetComponentInChildren<CameraController>();

        if (cameraController == null)
        {
            Debug.LogError("CameraController component not found in the FPSCameraPrefab.");
        }
        else
        {
            playerCameraObject.transform.SetParent(transform);
            cameraController.AttachCamera(transform);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Die()
    {
        Debug.Log("Player died locally.");
        canMove = false;
        DetachCamera();
    }

    private void DetachCamera()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(null);
            CameraController cameraController = playerCamera.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.DetachCamera();
            }

            playerCamera = null;
        }
    }

    private void OnDestroy()
    {
        if (damageSystem != null)
        {
            damageSystem.OnDeath -= Die;
        }
    }
}
