using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class NavMeshMovement : MovementController
{
    private Rigidbody rig;

    [SerializeField] private AnimationController animator;

    private bool isPathfinding;
    private Vector3 pathfindingDestination;
    private PlayerStats playerStats;
    
    public NavMeshAgent NavMeshAgent { get; private set; }

    public bool IsPathfinding => isPathfinding;

    public Vector3 PathfindingDestination 
    { 
        get => pathfindingDestination;
        set 
        { 
            pathfindingDestination = value;
            if (isPathfinding && NavMeshAgent != null) 
            {
                NavMeshAgent.SetDestination(pathfindingDestination);
            }
        } 
    }
    
    public override void SetMovementDisabled(bool isMovementDisabled)
    {
        base.SetMovementDisabled(isMovementDisabled);
        if (isMovementDisabled && isPathfinding) StopPathfinding();
        rig.isKinematic = isMovementDisabled;
    }


    protected override void Awake()
    {
        base.Awake();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        NavMeshAgent.enabled = false;
        rig = GetComponent<Rigidbody>();
        
        playerStats = GetComponent<PlayerStats>();
        playerStats.OnStatChanged += UpdateStats;
        playerStats.OnStatUpdated += UpdateStats;
        UpdateStats(playerStats);
    }

    private void OnDisable()
    {
        playerStats.OnStatChanged -= UpdateStats;
        playerStats.OnStatUpdated -= UpdateStats;
    }

    private void UpdateStats(PlayerStats playerStats)
    {
        NavMeshAgent.acceleration = playerStats.Acceleration.Value;
        NavMeshAgent.speed = playerStats.SprintSpeed.Value;
    }

    private void FixedUpdate()
    {
        if (!NetworkMovementController.IsServer) return;

        if (isPathfinding) UpdatePathfinding();
    }

    private void Update()
    {
        if (!NetworkMovementController.IsServer || !animator) return;
        
        Vector3 localVelocity = WorldToLocalVelocity(GetVelocity(), transform.rotation) / playerStats.SprintSpeed.BaseValue;
        animator.SetFloat("xinput", localVelocity.x);
        animator.SetFloat("yinput", localVelocity.z);
    }

    public override void ApplyForce(Vector3 force, ForceMode forceMode)
    {
        if (IsMovementDisabled) return;

        if (isPathfinding) NavMeshAgent.velocity += force * 0.1f;
        else rig.AddForce(force, forceMode);
    }

    public override Vector3 GetVelocity()
    {
        if (isPathfinding) return NavMeshAgent.velocity;
        else return rig.linearVelocity;
    }

    public override void SetVelocity(Vector3 velocity)
    {
        if (IsMovementDisabled) return;

        if (isPathfinding) NavMeshAgent.velocity = velocity;
        else rig.linearVelocity = velocity;
    }

    public bool CanPathfind(Vector3 position)
    {
        if (isPathfinding || IsMovementDisabled) return false;

        return NavMesh.SamplePosition(position, out _, NavMeshAgent.height * 2f, NavMesh.AllAreas);
    }

    public bool CanPathfind()
    {
        return CanPathfind(pathfindingDestination);
    }

    public void Pathfind(Vector3 position)
    {
        pathfindingDestination = position;
        if (CanPathfind())
        {
            NavMeshAgent.enabled = true;
            //navMeshAgent.updatePosition = false;
            NavMeshAgent.isStopped = false;
            NavMeshAgent.SetDestination(pathfindingDestination);
            NavMeshAgent.velocity = GetVelocity();
            SetVelocity(Vector3.zero);

            rig.isKinematic = true;

            isPathfinding = true;
        }
    }

    private void UpdatePathfinding()
    {
        if (IsInputDisabled != NavMeshAgent.isStopped) NavMeshAgent.isStopped = IsInputDisabled;
        if (NavMeshAgent.destination != pathfindingDestination) NavMeshAgent.SetDestination(pathfindingDestination);

        //if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid || (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending))
        //{
        //    StopPathfinding();
        //}
    }

    public void StopPathfinding()
    {
        if (!isPathfinding) return;

        NavMeshAgent.isStopped = true;
        //navMeshAgent.updatePosition = true;

        rig.isKinematic = false;

        rig.linearVelocity = GetVelocity();
        SetVelocity(Vector3.zero);

        isPathfinding = false;
        NavMeshAgent.enabled = false;
    }
}