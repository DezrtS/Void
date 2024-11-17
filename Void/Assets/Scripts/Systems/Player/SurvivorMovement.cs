using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class SurvivorController : NetworkBehaviour
{
    public Transform spawnPoint;
    public GameObject FPSCameraPrefab;
    public GameObject damageSpherePrefab;

    private float attackTimer = 2.0f;
    private float lastAttack;
    private NetworkVariable<bool> canDamage = new NetworkVariable<bool>(false);

    private Camera playerCamera;
    private CharacterController characterController;

    // Movement settings
    private float walkSpeed = 6f;
    private float runSpeed = 12f;
    private float jumpPower = 7f;
    private float gravity = 10f;
    private float lookSpeed = 2f;
    private float lookXLimit = 45f;
    private float defaultHeight = 2f;
    private float crouchHeight = 1f;
    private float crouchSpeed = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    private bool canMove = true;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            AlignToGround();
            SpawnPlayerCamera();
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        HandleMovement();
        HandleAttack();
    }

    private void HandleMovement()
    {
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

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove && playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
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

        GameObject instantiatedCamera = Instantiate(FPSCameraPrefab, spawnPoint.position, spawnPoint.rotation);
        playerCamera = instantiatedCamera.GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("Camera component not found in FPSCameraPrefab or its children.");
        }
        else
        {
            instantiatedCamera.transform.SetParent(transform);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void AlignToGround()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("SpawnPoint is not assigned.");
            return;
        }

        // Perform a raycast downwards from the spawn point to detect the ground.
        Ray ray = new Ray(spawnPoint.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 10f))
        {
            // If the raycast hits something, adjust the position slightly above the ground.
            float groundOffset = 0.1f;
            transform.position = hitInfo.point + Vector3.up * (characterController.height / 2 + groundOffset);
        }
        else
        {
            // If no ground is detected, we can fallback to spawnPoint, but adjust the height.
            Debug.LogWarning("No ground detected below the spawn point. Using fallback position.");

            // Adjust the position using the spawn point's y-axis value plus a slight offset.
            transform.position = spawnPoint.position + Vector3.up * (characterController.height / 2);
        }
    }


    private void HandleAttack()
    {
        lastAttack += Time.deltaTime;
        if (lastAttack > attackTimer)
        {
            SetCanDamageServerRpc(false);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            AttackServerRpc();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            StopAttackServerRpc();
        }
    }

    [ServerRpc]
    private void AttackServerRpc()
    {
        SetCanDamageServerRpc(true);
        SpawnDamageSphereServerRpc();
        lastAttack = 0.0f;
    }

    [ServerRpc]
    private void StopAttackServerRpc()
    {
        SetCanDamageServerRpc(false);
    }

    [ServerRpc]
    private void SetCanDamageServerRpc(bool value)
    {
        canDamage.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnDamageSphereServerRpc()
    {
        if (damageSpherePrefab == null || playerCamera == null)
        {
            Debug.LogError("Damage Sphere Prefab or Player Camera is not assigned!");
            return;
        }

        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * 2f;

        GameObject damageSphere = Instantiate(damageSpherePrefab, spawnPosition, Quaternion.identity);

        NetworkObject networkObject = damageSphere.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("Damage Sphere does not have a NetworkObject component!");
        }

        Debug.Log($"Damage Sphere instantiated at {spawnPosition} in the direction of the camera.");
    }


    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (playerCamera != null)
        {
            Destroy(playerCamera.gameObject);
        }
    }
}
