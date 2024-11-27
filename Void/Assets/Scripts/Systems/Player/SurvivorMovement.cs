using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class SurvivorController : NetworkBehaviour
{
    public Transform spawnPoint;
    public GameObject FPSCameraPrefab;

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
    private bool isDead = false; 

    private DamageSystem damageSystem;

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
            damageSystem.OnDeath += Die; 
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
        if (!IsOwner || !IsSpawned) return;

        if (isDead)
        {
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        if (isDead) return;

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
        Debug.Log("Die method triggered.");
        isDead = true;
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
