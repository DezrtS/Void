using UnityEngine;
using UnityEngine.AI;

namespace Assignment_1
{
    public class StopMoving : Task
    {
        private NavMeshAgent navMeshAgent;
        
        public override void Run(Blackboard blackboard)
        {
            navMeshAgent ??= blackboard.Get<NavMeshAgent>("NavMeshAgent");
            navMeshAgent.isStopped = true;
        }
    }
}
