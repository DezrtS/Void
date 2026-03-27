using UnityEngine;

namespace Assignment_2
{
    public class GAttackTarget : GAction
    {
        private readonly float minAttackDistance;
        private float initialHealth;

        public GAttackTarget(string ID, float minAttackDistance) : base(ID)
        {
            this.minAttackDistance = minAttackDistance;
        }
        
        public override bool CanHappen(Blackboard blackboard)
        {
            if (blackboard["Attack"] is not BasicAttack basicAttack || blackboard["Time"] is not float time || blackboard["Time At Last Attack"] is not float timeAtLastAttack || blackboard["Target"] is not Transform target || blackboard["Position"] is not Vector3 position)
                return false;
            
            return Vector3.Distance(target.position, position) <= minAttackDistance && time - timeAtLastAttack >= basicAttack.UseDuration;
        }

        public override Blackboard OnCompletion(Blackboard blackboard) 
        {
            var attack = (BasicAttack)blackboard["Attack"];
            blackboard["Target Health Value"] = (float)blackboard["Target Health Value"] - attack.Damage;
            blackboard["Time At Last Attack"] = blackboard["Time"];
            return blackboard;
        }

        public override void BeginAction(Blackboard blackboard)
        {
            var basicAttack = (BasicAttack)blackboard["Attack"];
            initialHealth = (float)blackboard["Target Health Value"];
            blackboard["Time At Last Attack"] = blackboard["Time"];
            basicAttack.RequestUse();
        }

        public override bool UpdateAction(Blackboard blackboard)
        {
            var currentHealth = ((Health)blackboard["Target Health"]).CurrentHealth;
            blackboard["Target Health Value"] = currentHealth;
            var basicAttack = (BasicAttack)blackboard["Attack"];
            if (basicAttack.IsAttacking) return true;
            basicAttack.RequestStopUsing();
            
            return currentHealth < initialHealth;
        }

        public override bool IsActionDone(Blackboard blackboard)
        {
            var currentHealth = ((Health)blackboard["Target Health"]).CurrentHealth;
            return currentHealth < initialHealth;
        }
    }
}