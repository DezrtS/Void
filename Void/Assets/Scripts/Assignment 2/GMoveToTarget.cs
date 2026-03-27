using UnityEngine;
using UnityEngine.AI;

namespace Assignment_2
{
    public class GMoveToTarget : GAction
    {
        private readonly float stopDistanceFromTarget;
        private readonly float secondsPerTick;

        private float movementTimer;

        public GMoveToTarget(string ID, float secondsPerTick, float stopDistanceFromTarget) : base(ID)
        {
            this.secondsPerTick = secondsPerTick;
            this.stopDistanceFromTarget = stopDistanceFromTarget;
        }
        
        public override bool CanHappen(Blackboard blackboard)
        {
            if (blackboard["MovementController"] is not NavMeshMovement || blackboard["Target"] is not Transform target ||
                blackboard["Position"] is not Vector3 position) return false;
            return Vector3.Distance(position, target.position) > stopDistanceFromTarget;
        }

        public override float Cost(Blackboard blackboard)
        {
            var position = (Vector3)blackboard["Position"];
            var target = blackboard["Target"] as Transform;
            var movementSpeed = (float)blackboard["Movement Speed"];
            var distance = Vector3.Distance(position, target.position);
            return distance / movementSpeed + 1;
        }

        public override Blackboard OnCompletion(Blackboard blackboard)
        {
            var position = (Vector3)blackboard["Position"];
            var target = blackboard["Target"] as Transform;
            var distance = Vector3.Distance(position, target.position);
            blackboard["Position"] = target.position;
            blackboard["Time"] = (float)blackboard["Time"] + distance / (float)blackboard["Movement Speed"];
            return blackboard;
        }

        public override void BeginAction(Blackboard blackboard)
        {
            movementTimer = 0;
        }

        public override bool UpdateAction(Blackboard blackboard)
        {
            movementTimer += Time.deltaTime + secondsPerTick;
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

            blackboard["Time"] = (float)blackboard["Time"] + movementTimer;
        }
    }
}