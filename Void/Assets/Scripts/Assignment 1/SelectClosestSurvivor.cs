using System.Collections.Generic;
using UnityEngine;

namespace Assignment_1
{
    public class SelectClosestSurvivor : BTNode
    {
        public SelectClosestSurvivor(string ID) : base(ID) {}

        public override STATUS tick(Blackboard blackboard)
        {
            if (blackboard["Transform"] is not Transform transform || blackboard["Targets"] is not List<Transform> targets) return STATUS.FAIL;

            var closestIndex = 0;
            var closestDistance = float.MaxValue;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                var distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance >= closestDistance) continue;
                
                closestDistance = distance;
                closestIndex = i;
            }
            
            blackboard["Target"] = targets[closestIndex];
            return STATUS.SUCCESS;
        }
    }
}