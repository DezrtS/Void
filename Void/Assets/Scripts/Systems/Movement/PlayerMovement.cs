using FMOD.Studio;
using FMODUnity;
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
    private bool isGrounded;
    private float defaultColliderHeight;
    private float footstepTimer;

    [SerializeField] private Transform orientationTransform;
    [SerializeField] private Transform groundedCheckTransform;
    [SerializeField] private float groundedCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundedCheckLayerMask;

    [Header("Acceleration")]
    [SerializeField] private Stat acceleration = new Stat(4);
    [SerializeField] private Stat timeToAccelerate = new Stat(0.25f);
    [SerializeField] private Stat timeToDeaccelerate = new Stat(0.25f);

    [Header("Speed")]
    [SerializeField] private Stat walkSpeed = new Stat(4);
    [SerializeField] private Stat sprintSpeed = new Stat(7);
    [SerializeField] private Stat crouchSpeed = new Stat(2);
    [SerializeField] private EventReference footstepSound;
    [SerializeField] private float footstepDelay;

    [Header("Jump")]
    [SerializeField] private bool canJump;
    [SerializeField] private Stat jumpPower = new Stat(7);
    [SerializeField] private EventReference jumpSound;
    [SerializeField] private EventReference landJumpSound;

    [Header("Crouch")]
    [SerializeField] private bool canCrouch;
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

    protected override void Awake()
    {
        base.Awake();
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

    public override void SetMovementDisabled(bool isMovementDisabled)
    {
        base.SetMovementDisabled(isMovementDisabled);
        rig.isKinematic = isMovementDisabled;
    }

    private void FixedUpdate()
    {
        if (!NetworkMovementController.IsOwner) return;
        bool newGroundedState = IsGrounded();
        if (!isGrounded && newGroundedState) AudioManager.RequestPlayOneShot(landJumpSound, transform.position);
        isGrounded = newGroundedState;
        HandleMovement();
    }

    private void Update()
    {
        if (!NetworkMovementController.IsOwner) return;

        Vector3 localVelocity = WorldToLocalVelocity(GetVelocity(), orientationTransform.rotation) / sprintSpeed.BaseValue;
        animationController.SetFloat("xinput", localVelocity.x);
        animationController.SetFloat("yinput", localVelocity.z);

        HandleFootstep();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = movementInputAction.ReadValue<Vector2>();
        if (IsInputDisabled) moveInput = Vector2.zero;

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

    private void HandleFootstep()
    {
        if (!isGrounded) return;
        float currentSpeed = rig.linearVelocity.magnitude;

        footstepTimer -= Time.deltaTime * currentSpeed;

        if (footstepTimer <= 0)
        {
            AudioManager.PlayOneShot(footstepSound, transform.position);
            footstepTimer = footstepDelay;
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!NetworkMovementController.IsOwner || IsInputDisabled || !canJump) return;

        if (!IsGrounded()) return;
        AudioManager.RequestPlayOneShot(jumpSound, transform.position);
        ApplyForce(Vector3.up * jumpPower.Value, ForceMode.VelocityChange);
    }

    private void Sprint(InputAction.CallbackContext context)
    {
        if (!NetworkMovementController.IsOwner || IsInputDisabled) return;
        isSprinting = context.performed;
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        if (!NetworkMovementController.IsOwner || IsInputDisabled || !canCrouch) return;
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
        if (IsMovementDisabled) return;
        rig.linearVelocity = velocity;
    }

    public override void ApplyForce(Vector3 force, ForceMode forceMode)
    {
        if (IsMovementDisabled) return;
        rig.AddForce(force, forceMode);
    }

    public bool IsGrounded()
    {
        Collider[] colliders = Physics.OverlapSphere(groundedCheckTransform.position, groundedCheckRadius, groundedCheckLayerMask, QueryTriggerInteraction.Ignore);
        return (colliders.Length > 0);
    }
}