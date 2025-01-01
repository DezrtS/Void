using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MovementController
{
    private Rigidbody rig;
    private CapsuleCollider capsuleCollider;
    private InputActionMap movementActionMap;
    private InputAction movementInputAction;

    private bool isSprinting;
    private bool isCrouched;
    private float defaultColliderHeight;

    [SerializeField] private Transform orientationTransform;

    [Header("Acceleration")]
    [SerializeField] private float timeToAccelerate = 0.25f;
    [SerializeField] private float timeToDeaccelerate = 0.25f;

    [Header("Speed")]
    [SerializeField] private float walkSpeed = 4;
    [SerializeField] private float sprintSpeed = 7;
    [SerializeField] private float crouchSpeed = 2;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 7;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1;

    private void OnEnable()
    {
        movementActionMap ??= InputSystem.actions.FindActionMap("Movement");
        movementActionMap.Enable();
        
        movementInputAction = movementActionMap.FindAction("Movement");

        InputAction jumpInputAction = movementActionMap.FindAction("Jump");
        jumpInputAction.performed += Jump;

        InputAction sprintInputAction = movementActionMap.FindAction("Sprint");
        sprintInputAction.performed += Sprint;
        sprintInputAction.canceled += Sprint;

        InputAction crouchInputAction = movementActionMap.FindAction("Crouch");
        crouchInputAction.performed += Crouch;
        crouchInputAction.canceled += Crouch;
    }

    private void OnDisable()
    {
        InputAction jumpInputAction = movementActionMap.FindAction("Jump");
        jumpInputAction.performed -= Jump;

        InputAction sprintInputAction = movementActionMap.FindAction("Sprint");
        sprintInputAction.performed -= Sprint;
        sprintInputAction.canceled -= Sprint;

        InputAction crouchInputAction = movementActionMap.FindAction("Crouch");
        crouchInputAction.performed -= Crouch;
        crouchInputAction.canceled -= Crouch;

        movementActionMap.Disable();
    }

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        defaultColliderHeight = capsuleCollider.height;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = movementInputAction.ReadValue<Vector2>();
        if (IsDisabled) moveInput = Vector2.zero;

        Vector3 move = orientationTransform.forward * moveInput.y + orientationTransform.right * moveInput.x;
        move.y = 0;

        float speed = walkSpeed;
        if (isCrouched)
        {
            speed = crouchSpeed;
        }
        else if (isSprinting)
        {
            speed = sprintSpeed;
        }

        Vector3 targetVelocity = move.normalized * speed;
        float targetSpeed = targetVelocity.magnitude;

        Vector3 velocityDifference = targetVelocity - rig.linearVelocity;
        velocityDifference.y = 0;
        Vector3 differenceDirection = velocityDifference.normalized;

        float accelerationIncrement;

        if (rig.linearVelocity.magnitude <= targetSpeed)
        {
            accelerationIncrement = GetAcceleration(sprintSpeed, timeToAccelerate) * Time.deltaTime;
        }
        else
        {
            accelerationIncrement = GetAcceleration(sprintSpeed, timeToDeaccelerate) * Time.deltaTime;
        }

        if (velocityDifference.magnitude < accelerationIncrement)
        {
            rig.AddForce(velocityDifference, ForceMode.VelocityChange);
        }
        else
        {
            rig.AddForce(differenceDirection * accelerationIncrement, ForceMode.VelocityChange);
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        rig.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
    }

    private void Sprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        isCrouched = context.performed;
        
        if (isCrouched)
        {
            capsuleCollider.height = crouchHeight;
        }
        else
        {
            capsuleCollider.height = defaultColliderHeight;
        }
    }

    public override Vector3 GetVelocity()
    {
        return rig.linearVelocity;
    }

    public override void SetVelocity(Vector3 velocity)
    {
        rig.linearVelocity = velocity;
    }

    public override Quaternion GetRotation()
    {
        return transform.rotation;
    }

    public override void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public override void ApplyForce(Vector3 force, ForceMode forceMode)
    {
        rig.AddForce(force, forceMode);
    }
}