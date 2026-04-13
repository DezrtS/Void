using Final_Assignment;
using UnityEngine;

namespace Assignment_2
{
    public class GFleeToSafePoint : GAction
    {
        private readonly float stopDistanceFromTarget;
        private readonly float secondsPerTick;

        private float movementTimer;

        public GFleeToSafePoint(string ID, float secondsPerTick, float stopDistanceFromTarget) : base(ID)
        {
            this.secondsPerTick = secondsPerTick;
            this.stopDistanceFromTarget = stopDistanceFromTarget;
        }
        
        public override bool CanHappen(Blackboard blackboard)
        {
            if (blackboard["MovementController"] is not NavMeshMovement || blackboard["SafePoint"] is not SafePoint safePoint ||
                blackboard["Position"] is not Vector3 position) return false;
            return Vector3.Distance(position, safePoint.transform.position) > stopDistanceFromTarget;
        }

        public override float Cost(Blackboard blackboard)
        {
            var position = (Vector3)blackboard["Position"];
            var safePoint = (SafePoint)blackboard["SafePoint"];
            var movementSpeed = (float)blackboard["Movement Speed"];
            var distance = Vector3.Distance(position, safePoint.transform.position);
            return distance / movementSpeed + 1;
        }

        public override Blackboard OnCompletion(Blackboard blackboard)
        {
            var position = (Vector3)blackboard["Position"];
            var safePoint = (SafePoint)blackboard["SafePoint"];
            var distance = Vector3.Distance(position, safePoint.transform.position);
            blackboard["Position"] = safePoint.transform.position;
            blackboard["Time"] = (float)blackboard["Time"] + distance / (float)blackboard["Movement Speed"];
            return blackboard;
        }

        public override void BeginAction(Blackboard blackboard)
        {
            base.BeginAction(blackboard);
            movementTimer = 0;
        }

        public override bool UpdateAction(Blackboard blackboard)
        {
            movementTimer += Time.deltaTime + secondsPerTick;
            var navMeshMovement = blackboard["MovementController"] as NavMeshMovement;
            var safePoint = (SafePoint)blackboard["SafePoint"];
            navMeshMovement.Pathfind(safePoint.transform.position);
            blackboard["Position"] = navMeshMovement.transform.position;
            return true;
        }

        public override bool IsActionDone(Blackboard blackboard)
        {
            var navMeshMovement = blackboard["MovementController"] as NavMeshMovement;
            var safePoint = (SafePoint)blackboard["SafePoint"];
            var distance = Vector3.Distance(navMeshMovement.transform.position, safePoint.transform.position);
            return !(distance > stopDistanceFromTarget);
        }

        public override void EndAction(Blackboard blackboard)
        {
            base.EndAction(blackboard);
            var navMeshMovement = blackboard["MovementController"] as NavMeshMovement;
            navMeshMovement.StopPathfinding();

            blackboard["Time"] = (float)blackboard["Time"] + movementTimer;
        }
    }
}