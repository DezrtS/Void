using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class SurvivorController : NetworkBehaviour
{
    public Transform spawnPoint; // Reference to the camera spawn point
    public GameObject FPSCameraPrefab; 
    public int playerHP = 100;

    private Camera playerCamera;
    private CharacterController characterController;

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

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            SpawnPlayerCamera();
        }
    }

    private void SpawnPlayerCamera()
    {
        if (FPSCameraPrefab == null || spawnPoint == null)
        {
            Debug.LogError("FPSCameraPrefab or spawnPoint is not assigned in the Inspector.");
            return;
        }

        // Instantiate the camera prefab at the exact location of the spawnPoint
        GameObject instantiatedCamera = Instantiate(FPSCameraPrefab, spawnPoint.position, spawnPoint.rotation);
        playerCamera = instantiatedCamera.GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("Camera component not found in FPSCameraPrefab or its children.");
        }
        else
        {
            Debug.Log("Camera component successfully assigned.");
            // Parent the instantiated camera to the player transform
            instantiatedCamera.transform.SetParent(transform);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // FPS Movement
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

        // Crouch control
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

            float rotationY = Input.GetAxis("Mouse X") * lookSpeed;
            transform.Rotate(0, rotationY, 0);
        }
    }
}
