using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLook : MonoBehaviour
{
    public Transform playerBody;
    public Transform cameraBody;

    [SerializeField] float mouseSensitivity = 2000;
    float cameraDistance = -10;
    float xRotation = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85, 85);

        cameraBody.localRotation = Quaternion.Euler(xRotation, 0, 0);
        playerBody.Rotate(Vector3.up * mouseX);

    }
}
