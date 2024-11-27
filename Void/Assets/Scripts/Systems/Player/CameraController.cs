using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera playerCamera; 
    public Transform playerTransform; 

    private float lookSpeed = 2f; 
    private float lookXLimit = 45f; 
    private float rotationX = 0f;  

    private bool isDetached = false; 
    private DamageSystem damageSystem;

    void Start()
    {
        damageSystem = playerTransform.GetComponent<DamageSystem>();

        if (damageSystem != null)
        {
            damageSystem.OnDeath += DetachCameraOnDeath; 
        }
        else
        {
            Debug.LogWarning("DamageSystem not found on player!");
        }
    }

    void Update()
    {
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

    private void DetachCameraOnDeath()
    {
        DetachCamera();
        Debug.Log("Player is dead, camera detached.");
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

    private void OnDestroy()
    {
        if (damageSystem != null)
        {
            damageSystem.OnDeath -= DetachCameraOnDeath;
        }
    }
}
