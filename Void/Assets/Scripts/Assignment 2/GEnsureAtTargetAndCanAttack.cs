using UnityEngine;

namespace Assignment_2
{
    public class GEnsureAtTargetAndCanAttack : GAction
    {
        private readonly float stopDistanceFromTarget;
        private readonly float secondsPerTick;
        private readonly float attackCooldown;

        public GEnsureAtTargetAndCanAttack(string ID, float secondsPerTick, float stopDistanceFromTarget, float attackCooldown) : base(ID)
        {
            this.secondsPerTick = secondsPerTick;
            this.stopDistanceFromTarget = stopDistanceFromTarget;
            this.attackCooldown = attackCooldown;
        }
        
        public override bool CanHappen(Blackboard blackboard)
        {
            if (blackboard["Is At Target And Can Attack"] is not bool isAtTargetAndCanAttack) return false;
            return !isAtTargetAndCanAttack;
        }

        public override float Cost(Blackboard blackboard)
        {
            var time = (float)blackboard["Time"];
            var timeAtLastAttack = (float)blackboard["Time At Last Attack"];
            var timeUntilNextAttack = Mathf.Max(0, attackCooldown - (time - timeAtLastAttack));
            
            var position = (Vector3)blackboard["Position"];
            var target = blackboard["Target"] as Transform;
            var movementSpeed = (float)blackboard["Movement Speed"];
            var distance = Vector3.Distance(position, target.position);
            return distance / movementSpeed + timeUntilNextAttack + 1;
        }

        public override Blackboard OnCompletion(Blackboard blackboard)
        {
            var target = blackboard["Target"] as Transform;
            blackboard["Position"] = target.position;
            blackboard["Time"] = (float)blackboard["Time"] + attackCooldown;
            blackboard["Is At Target And Can Attack"] = true;
            return blackboard;
        }

        public override bool UpdateAction(Blackboard blackboard)
        {
            blackboard["Time"] = (float)blackboard["Time"] + Time.deltaTime + secondsPerTick;
            var navMeshMovement = blackboard["MovementController"] as NavMeshMovement;
            var target = blackboard["Target"] as Transform;
            navMeshMovement.Pathfind(target.position);
            blackboard["Position"] = navMeshMovement.transform.position;
            return true;
        }

        public override bool IsActionDone(Blackboard blackboard)
        {
            var navMeshMovement = (NavMeshMovement)blackboard["MovementController"];
            var target = (Transform)blackboard["Target"];
            var time = (float)blackboard["Time"];
            var timeAtLastAttack = (float)blackboard["Time At Last Attack"];
            
            var distanceFromTarget = Vector3.Distance(navMeshMovement.transform.position, target.position);
            var timeSinceAttack = time - timeAtLastAttack;

            return distanceFromTarget <= stopDistanceFromTarget && timeSinceAttack > attackCooldown;
        }

        public override void EndAction(Blackboard blackboard)
        {
            base.EndAction(blackboard);
            var navMeshMovement = blackboard["MovementController"] as NavMeshMovement;
            navMeshMovement.StopPathfinding();
        }
    }
}