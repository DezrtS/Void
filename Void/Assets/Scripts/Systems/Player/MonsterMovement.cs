using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class MonsterMovement : NetworkBehaviour
{
    // Monster stats
    private float attackTimer = 2.0f;
    private float lastAttack;
    private NetworkVariable<bool> canDamage = new NetworkVariable<bool>(false);

    public GameObject damageSpherePrefab;

    // Monster movement
    private Camera playerCamera;
    private float walkSpeed = 8f;
    private float runSpeed = 16f;
    private float jumpPower = 7f;
    private float gravity = 10f;
    private float lookSpeed = 2f;
    private float lookXLimit = 45f;
    private float defaultHeight = 3f;
    private float crouchHeight = 2f;
    private float crouchSpeed = 5f;
    public Transform spawnPoint;
    public GameObject FPSCameraPrefab;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private bool canMove = true;

    private Animator animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (!IsOwner) return;
        
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (FPSCameraPrefab == null)
        {
            Debug.LogError("FPSCameraPrefab is not assigned in the Inspector.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint is not assigned in the Inspector.");
            return;
        }

        GameObject instantiatedCamera = Instantiate(FPSCameraPrefab, spawnPoint.position + Vector3.up * 0.5f, spawnPoint.rotation);
        playerCamera = instantiatedCamera.GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("Camera component not found in FPSCameraPrefab.");
            return;
        }

        instantiatedCamera.transform.SetParent(transform);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        if(curSpeedX > 0 || curSpeedY > 0)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

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

        // Handle crouching
        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 8f;
            runSpeed = 16f;
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
            animator.SetBool("attack", true);

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

    [ServerRpc]
    private void SpawnDamageSphereServerRpc()
    {
        if (damageSpherePrefab == null || playerCamera == null)
        {
            Debug.LogError("Damage Sphere Prefab or Player Camera is not assigned!");
            return;
        }

        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * 2f;

        GameObject damageSphere = Instantiate(damageSpherePrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Damage Sphere instantiated at {spawnPosition} in the direction of the camera.");

        NetworkObject networkObject = damageSphere.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("Damage Sphere does not have a NetworkObject component!");
        }
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