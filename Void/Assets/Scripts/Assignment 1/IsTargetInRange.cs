using UnityEngine;

namespace Assignment_1
{
    public class IsTargetInRange : BTNode
    {
        private readonly float range;
        
        public IsTargetInRange(string ID, float range) : base(ID)
        {
            this.range = range;
        }

        public override STATUS tick(Blackboard blackboard)
        {
            if (blackboard["Transform"] is not Transform transform || blackboard["Target"] is not Transform target) return STATUS.FAIL;
            
            var distance = Vector3.Distance(transform.position, target.position);
            return distance <= range ? STATUS.SUCCESS : STATUS.FAIL;
        }
    }
}