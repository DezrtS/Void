using UnityEngine;
using UnityEngine.AI;

namespace Assignment_2
{
    public class GMoveToTarget : GAction
    {
        private readonly float stopDistanceFromTarget;

        public GMoveToTarget(string ID, float stopDistanceFromTarget) : base(ID)
        {
            this.stopDistanceFromTarget = stopDistanceFromTarget;
        }
        
        public override bool CanHappen(Blackboard blackboard)
        {
            return blackboard["MovementController"] is NavMeshMovement && blackboard["Target"] is Transform && blackboard["Position"] is Vector3;
        }

        public override float Cost(Blackboard blackboard)
        {
            var navMeshMovement = blackboard["MovementController"] as NavMeshMovement;
            var target = blackboard["Target"] as Transform;
            var distance = Vector3.Distance(navMeshMovement.transform.position, target.position);
            return distance;
        }

        public override Blackboard OnCompletion(Blackboard blackboard)
        {
            blackboard["Position"] = ((Transform)blackboard["Target"]).position;
            return blackboard;
        }

        public override bool UpdateAction(Blackboard blackboard)
        {
            var navMeshMovement = blackboard["MovementController"] as NavMeshMovement;
            var target = blackboard["Target"] as Transform;
            navMeshMovement.Pathfind(target.position);
            blackboard["Position"] = navMeshMovement.transform.position;
            return true;
        }

        public override bool IsActionDone(Blackboard blackboard)
        {
            var navMeshMovement = blackboard["MovementController"] as NavMeshMovement;
            var target = blackboard["Target"] as Transform;
            var distance = Vector3.Distance(navMeshMovement.transform.position, target.position);
            return !(distance > stopDistanceFromTarget);
        }

        public override void EndAction(Blackboard blackboard)
        {
            var navMeshMovement = blackboard["MovementController"] as NavMeshMovement;
            navMeshMovement.StopPathfinding();
        }
    }
}