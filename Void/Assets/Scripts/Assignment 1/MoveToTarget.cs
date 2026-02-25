using UnityEngine;
using UnityEngine.AI;

namespace Assignment_1
{
    public class MoveToTarget : BTNode
    {
        private readonly float stopDistanceFromTarget;

        public MoveToTarget(string ID, float stopDistanceFromTarget) : base(ID)
        {
            this.stopDistanceFromTarget = stopDistanceFromTarget;
        }

        public override STATUS tick(Blackboard blackboard)
        {
            if (blackboard["MovementController"] is not NavMeshMovement navMeshMovement || blackboard["Target"] is not Transform target) return STATUS.FAIL;

            navMeshMovement.Pathfind(target.position);
            var distance = Vector3.Distance(navMeshMovement.transform.position, target.position);
            if (distance > stopDistanceFromTarget) return STATUS.RUNNING;
            navMeshMovement.StopPathfinding();
            return STATUS.SUCCESS;

        }
    }
}