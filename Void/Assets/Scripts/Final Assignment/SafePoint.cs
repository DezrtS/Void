using System.Collections.Generic;
using UnityEngine;

namespace Final_Assignment
{
    public class SafePoint : MonoBehaviour
    {
        [SerializeField] private float checkRadius;
        [SerializeField] private LayerMask checkLayerMask;
        [SerializeField] private LayerMask collisionLayerMask;
        
        public bool IsPointSafe()
        {
            var results = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(transform.position, checkRadius, results, checkLayerMask);
            
            for (var i = 0; i < size; i++)
            {
                var target = results[i];
                if (!target.CompareTag("Player")) continue;
                var displacement = target.transform.position - transform.position;
                if (!Physics.Raycast(transform.position, displacement.normalized, out RaycastHit hit, checkRadius, collisionLayerMask)) continue;
                if (hit.collider.CompareTag("Player")) return false;
            }

            return true;
        }
    }
}
