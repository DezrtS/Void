using UnityEngine;

namespace Assignment_1
{
    public class IsSafe : BTNode
    {
        private float checkRadius;
        private LayerMask checkLayerMask;
        private LayerMask collisionLayerMask;
        
        public IsSafe(string ID, float checkRadius, LayerMask checkLayerMask, LayerMask collisionLayerMask) : base(ID)
        {
            this.checkRadius = checkRadius;
            this.checkLayerMask = checkLayerMask;
            this.collisionLayerMask = collisionLayerMask;
        }

        public override STATUS tick(ref Blackboard blackboard)
        {
            if (blackboard["Transform"] is not Transform transform) return STATUS.FAIL;
            
            var results = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(transform.position, checkRadius, results, checkLayerMask);
            
            for (var i = 0; i < size; i++)
            {
                var target = results[i];
                if (!target.CompareTag("Player")) continue;
                var displacement = target.transform.position - transform.position;
                var direction = displacement.normalized;
                if (!Physics.Raycast(transform.position + direction * 2f, direction, out RaycastHit hit, checkRadius, collisionLayerMask)) continue;
                if (hit.collider.CompareTag("Player")) return STATUS.FAIL;
            }

            return STATUS.SUCCESS;
        }
    }
}