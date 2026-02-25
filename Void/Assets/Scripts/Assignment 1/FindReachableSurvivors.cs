using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Assignment_1
{
    public class FindReachableSurvivors : BTNode
    {
        private readonly float searchRadius;
        private readonly LayerMask searchLayerMask;
        
        public FindReachableSurvivors(string ID, float searchRadius, LayerMask searchLayerMask) : base(ID)
        {
            this.searchRadius = searchRadius;
            this.searchLayerMask = searchLayerMask;
        }

        public override STATUS tick(Blackboard blackboard)
        {
            if (blackboard["MovementController"] is not NavMeshMovement navMeshMovement ) return STATUS.FAIL;
            var results = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(navMeshMovement.transform.position, searchRadius, results, searchLayerMask);
            
            var foundTargets = new List<Transform>();
            for (var i = 0; i < size; i++)
            {
                var target = results[i];
                if (!target.CompareTag("Player")) continue;
                if (navMeshMovement.CanPathfind(target.transform.position))
                {
                    foundTargets.Add(target.transform);
                }
            }
            
            blackboard["Targets"] = foundTargets;
            return foundTargets.Count <= 0 ? STATUS.FAIL : STATUS.SUCCESS;
        }
    }
}