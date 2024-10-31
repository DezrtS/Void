using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class MonsterMovement : NetworkBehaviour
{
    //monster stats
    public GameObject attackRadius;
    public float meleeDamage = 15;
    public int monsterHP = 350;
    private float attackTimer = 2.0f;
    private float lastAttack;
    private bool canDamage = false;

    //monster movement
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

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (IsOwner)
        {
            if (FPSCameraPrefab == null)
            {
                Debug.LogError("FPSCameraPrefab is not assigned in the Inspector.");
                return;
            }

            GameObject instantiatedCamera = Instantiate(FPSCameraPrefab, spawnPoint.position, transform.rotation);
            playerCamera = instantiatedCamera.GetComponentInChildren<Camera>();

            // Debugging to check if the camera component is found
            if (playerCamera == null)
            {
                Debug.LogError("Camera component not found in FPSCameraPrefab or its children. Please ensure FPSCameraPrefab has a Camera component.");
            }
            else
            {
                Debug.Log("Camera component successfully assigned.");
            }

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

        lastAttack += Time.deltaTime;
        if (lastAttack > attackTimer)
        {
            canDamage = false;
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            Attack();
            StopAttack();
        }
    } 

    void Attack()
    {
        canDamage = true;
        attackRadius.SetActive(true);
        lastAttack = 0.0f;
    }
    
    void StopAttack()
    {
        canDamage = false;
        attackRadius.SetActive(false);
    }
}