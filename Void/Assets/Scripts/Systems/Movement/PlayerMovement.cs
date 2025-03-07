using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MovementController
{
    private Rigidbody rig;
    private CapsuleCollider capsuleCollider;

    AnimationController animationController;

    private InputActionMap movementActionMap;
    private InputAction movementInputAction;

    private bool isSprinting;
    private bool isCrouched;
    private float defaultColliderHeight;

    [SerializeField] private Transform orientationTransform;

    [Header("Acceleration")]
    [SerializeField] private Stat acceleration = new Stat(4);
    [SerializeField] private Stat timeToAccelerate = new Stat(0.25f);
    [SerializeField] private Stat timeToDeaccelerate = new Stat(0.25f);

    [Header("Speed")]
    [SerializeField] private Stat walkSpeed = new Stat(4);
    [SerializeField] private Stat sprintSpeed = new Stat(7);
    [SerializeField] private Stat crouchSpeed = new Stat(2);

    [Header("Jump")]
    [SerializeField] private Stat jumpPower = new Stat(7);

    [Header("Crouch")]
    [SerializeField] private Stat crouchHeight = new Stat(1);

    public Stat TimeToAccelerate => timeToAccelerate;
    public Stat TimeToDeacclerate => timeToDeaccelerate;

    public Stat WalkSpeed => walkSpeed;
    public Stat SprintSpeed => sprintSpeed;
    public Stat CrouchSpeed => crouchSpeed;

    public Stat JumpPower => jumpPower;

    public Stat CrouchHeight => crouchHeight;

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

        UIManager.OnPause += (bool paused) =>
        {
            if (paused)
            {
                movementActionMap.Disable();
            }
            else
            {
                movementActionMap.Enable();
            }
        };
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

        animationController = GetComponent<AnimationController>();

        if (TryGetComponent(out PlayerStats playerStats))
        {
            acceleration = playerStats.Acceleration;
            timeToAccelerate = playerStats.TimeToAccelerate;
            timeToDeaccelerate = playerStats.TimeToDeacclerate;

            walkSpeed = playerStats.WalkSpeed;
            sprintSpeed = playerStats.SprintSpeed;
            crouchSpeed = playerStats.CrouchSpeed;

            jumpPower = playerStats.JumpPower;

            crouchHeight = playerStats.CrouchHeight;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        HandleMovement();
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector3 localVelocity = WorldToLocalVelocity(GetVelocity(), orientationTransform.rotation) / sprintSpeed.BaseValue;
        animationController.SetFloat("xinput", localVelocity.x);
        animationController.SetFloat("yinput", localVelocity.z);
    }

    private void HandleMovement()
    {
        Vector2 moveInput = movementInputAction.ReadValue<Vector2>();
        if (IsDisabled) moveInput = Vector2.zero;

        Vector3 move = orientationTransform.forward * moveInput.y + orientationTransform.right * moveInput.x;
        move.y = 0;

        float speed = walkSpeed.Value;
        if (isCrouched)
        {
            speed = crouchSpeed.Value;
        }
        else if (isSprinting)
        {
            speed = sprintSpeed.Value;
        }

        Vector3 velocityChange = HandleMovement(move, speed, acceleration.Value, timeToAccelerate.Value, timeToDeaccelerate.Value, rig.linearVelocity);
        ApplyForce(velocityChange, ForceMode.VelocityChange);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!IsOwner || IsDisabled) return;
        rig.AddForce(Vector3.up * jumpPower.Value, ForceMode.VelocityChange);
    }

    private void Sprint(InputAction.CallbackContext context)
    {
        if (!IsOwner || IsDisabled) return;
        isSprinting = context.performed;
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        if (!IsOwner || IsDisabled) return;
        isCrouched = context.performed;
        
        if (isCrouched)
        {
            capsuleCollider.height = crouchHeight.Value;
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

    public override void ApplyForce(Vector3 force, ForceMode forceMode)
    {
        rig.AddForce(force, forceMode);
    }
}