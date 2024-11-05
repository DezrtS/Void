using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TestMovement : MonoBehaviour
{

    public Transform playerBody;
    public CharacterController playerController;

    private float speed = 12;
    private float strafeSpeed = 12;
    private float sprintMultiplier;
    private float gravity = -19.62f;
    private Vector3 velocity;

    public Transform groundCheck;
    private float groundDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;
    private bool canJump = true;
    private float jumpHeight = 3;

    private float maxClimbHeight = 1.5f;
    private float minClimbHeight = -1f;
    private float climbReach = 2;
    private int climbPasses = 20;

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1.5f;
        }

        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift) && zInput > 0)
        {
            sprintMultiplier = 2;
        }
        else
        {
            sprintMultiplier = 1;
        }
        Vector3 movement = transform.right * xInput * strafeSpeed + transform.forward * zInput * speed * sprintMultiplier;

        playerController.Move(movement * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && canJump)
        {
            float ledgeHeight = minClimbHeight;
            float lastMin = ledgeHeight;
            float lastMax = maxClimbHeight;
            if (!Physics.Raycast(transform.position + new Vector3(0, maxClimbHeight, 0), transform.TransformDirection(Vector3.forward), climbReach, groundMask) && Physics.Raycast(transform.position + new Vector3(0, minClimbHeight, 0), transform.TransformDirection(Vector3.forward), climbReach, groundMask))
            {
                velocity.y = 0;
                for (int i = 0; i < climbPasses; i++)
                {
                    if (Physics.Raycast(transform.position + new Vector3(0, ledgeHeight, 0), transform.TransformDirection(Vector3.forward), climbReach, groundMask))
                    {
                        lastMin = ledgeHeight;
                        ledgeHeight = (ledgeHeight + lastMax) / 2;
                    }
                    else
                    {
                        lastMax = ledgeHeight;
                        ledgeHeight = (ledgeHeight + lastMin) / 2;
                    }
                }
                playerController.enabled = false;
                transform.position = transform.position + new Vector3(0, ledgeHeight + playerController.height / 2, 0) + transform.TransformDirection(Vector3.forward) * climbReach;
                playerController.enabled = true;
            }
            else if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        playerController.Move(velocity * Time.deltaTime);

        //Denzil2004
    }
}
