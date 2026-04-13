using System.Linq;
using UnityEngine;

namespace Assignment_1
{
    public class SelectClosestSafePoint : BTNode
    {
        public SelectClosestSafePoint(string ID) : base(ID) {}

        public override STATUS tick(ref Blackboard blackboard)
        {
            if (blackboard["Transform"] is not Transform transform) return STATUS.FAIL;
            
            var orderedEnumerable = ElevatorManager.Instance.SafePoints.OrderBy(sp => (sp.transform.position - transform.position).magnitude);
            foreach (var safePoint in orderedEnumerable)
            {
                if (!safePoint.IsPointSafe()) continue;
                blackboard["SafePoint"] = safePoint.transform;
                return STATUS.SUCCESS;
            }

            return STATUS.FAIL;
        }
    }
}