using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera playerCamera;
    public Transform playerTransform;

    private float lookSpeed = 2f;
    private float lookXLimit = 45f;
    private float rotationX = 0f;

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

    public void DetachCamera()
    {
        playerTransform = null;
        playerCamera.transform.SetParent(null); 
    }

    public void AttachCamera(Transform player)
    {
        playerTransform = player;
        playerCamera.transform.SetParent(player); 
    }
}

