using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class NavMeshMovement : MovementController
{
    private NavMeshAgent navMeshAgent;
    private Rigidbody rig;

    [SerializeField] private Animator animator;

    private bool isPathfinding;
    private Vector3 pathfindingDestination;

    public bool IsPathfinding => isPathfinding;
    //public Vector3 PathfindingDestination { get { return pathfindingDestination; } set { pathfindingDestination = value; } }

    public Vector3 PathfindingDestination 
    { 
        get { return pathfindingDestination; } 
        set 
        { 
            pathfindingDestination = value;
            if (isPathfinding && navMeshAgent != null) 
            {
                navMeshAgent.SetDestination(pathfindingDestination);
            }
        } 
    }

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.enabled = false;
        rig = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!NetworkMovementController.IsServer) return;

        if (isPathfinding) UpdatePathfinding();
    }

    private void Update()
    {
        if (!NetworkMovementController.IsServer) return;

        Vector3 velocity = GetVelocity();
        velocity.y = 0;
        animator.SetFloat("Speed", velocity.magnitude / navMeshAgent.speed);
    }

    public override void ApplyForce(Vector3 force, ForceMode forceMode)
    {
        if (IsMovementDisabled) return;

        if (isPathfinding) navMeshAgent.velocity += force * 0.1f;
        else rig.AddForce(force, forceMode);
    }

    public override Vector3 GetVelocity()
    {
        if (isPathfinding) return navMeshAgent.velocity;
        else return rig.linearVelocity;
    }

    public override void SetVelocity(Vector3 velocity)
    {
        if (IsMovementDisabled) return;

        if (isPathfinding) navMeshAgent.velocity = velocity;
        else rig.linearVelocity = velocity;
    }

    public bool CanPathfind(Vector3 position)
    {
        if (isPathfinding) return false;

        return NavMesh.SamplePosition(position, out _, 5, NavMesh.AllAreas);
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
            navMeshAgent.enabled = true;
            //navMeshAgent.updatePosition = false;
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(pathfindingDestination);
            navMeshAgent.velocity = GetVelocity();
            SetVelocity(Vector3.zero);

            rig.isKinematic = true;

            isPathfinding = true;
        }
    }

    private void UpdatePathfinding()
    {
        if (IsInputDisabled != navMeshAgent.isStopped) navMeshAgent.isStopped = IsInputDisabled;
        if (navMeshAgent.destination != pathfindingDestination) navMeshAgent.SetDestination(pathfindingDestination);

        //if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid || (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending))
        //{
        //    StopPathfinding();
        //}
    }

    public void StopPathfinding()
    {
        if (!isPathfinding) return;

        navMeshAgent.isStopped = true;
        //navMeshAgent.updatePosition = true;

        rig.isKinematic = false;

        rig.linearVelocity = GetVelocity();
        SetVelocity(Vector3.zero);

        isPathfinding = false;
        navMeshAgent.enabled = false;
    }
}