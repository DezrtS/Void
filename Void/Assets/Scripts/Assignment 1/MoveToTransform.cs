using UnityEngine;
using UnityEngine.AI;

namespace Assignment_1
{
    public class MoveToTransform : Task
    {
        private NavMeshAgent navMeshAgent;
        
        public override void Run(Blackboard blackboard)
        {
            navMeshAgent ??= blackboard.Get<NavMeshAgent>("NavMeshAgent");
            blackboard.Get<Transform>("TargetTransform", out var targetTransform);
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(targetTransform.position);
        }
    }
}
