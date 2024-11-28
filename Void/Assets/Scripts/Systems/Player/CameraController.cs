using UnityEngine;
using System;
using Unity.Netcode;

public class CameraController : NetworkBehaviour
{
    public Camera playerCamera;
    public Transform playerTransform;

    private float lookSpeed = 2f;
    private float lookXLimit = 45f;
    private float rotationX = 0f;

    private bool isDetached = false;

    private DamageSystem damageSystem;

    private void Start()
    {
        damageSystem = GetComponentInParent<DamageSystem>();

        if (damageSystem == null)
        {
            Debug.LogWarning("DamageSystem not found on parent!");
        }
    }

    void Update()
    {
        if (damageSystem != null && damageSystem.isDead.Value)
        {
            if (isDetached)
            {
                HandleIndependentCameraMovement();
            }
            return;
        }

        if (playerTransform != null && !isDetached)
        {
            FollowPlayer();
            HandleCameraMovement();
        }
        else if (isDetached)
        {
            HandleIndependentCameraMovement();
        }
    }


    private void FollowPlayer()
    {
        transform.position = playerTransform.position;
        transform.rotation = playerTransform.rotation;
    }

    public void HandleCameraMovement()
    {
        if (playerTransform != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            playerTransform.Rotate(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
        else
        {
            Debug.LogWarning("Player Transform is not assigned in CameraController!");
        }
    }

    private void HandleIndependentCameraMovement()
    {
        float horizontalInput = Input.GetAxis("Mouse X");
        float verticalInput = -Input.GetAxis("Mouse Y");

        transform.Rotate(verticalInput * lookSpeed, horizontalInput * lookSpeed, 0);
    }

    public void DetachCamera()
    {
        playerTransform = null;
        isDetached = true;
    }

    public void AttachCamera(Transform player)
    {
        playerTransform = player;
        isDetached = false;
    }
}
