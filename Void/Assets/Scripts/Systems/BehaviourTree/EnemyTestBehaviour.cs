using System;
using UnityEngine;
using UnityEngine.AI;

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyTestBehaviour : MonoBehaviour
    {
        BTRoot behaviourTree;
        BTNode.STATUS treeStatus = BTNode.STATUS.RUNNING;
        Blackboard blackboard = new Blackboard();
        [SerializeField] private GameObject target;
        
        public NavMeshAgent agent;
        
        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();

            behaviourTree = new BTRoot("Root");
            Sequence goToSequence = new Sequence("goToSequence");
            blackboard["target"] = target;
            Leaf goToTarget = new Leaf("goToTarget",this.goToTarget);
            Leaf stayatTarget = new Leaf("stayatTarget",this.stayatTarget);
            
            goToSequence.children.Add(goToTarget);
            goToSequence.children.Add(stayatTarget);
            behaviourTree.children.Add(goToSequence);
        }

        

        BTNode.STATUS goToTarget()
        {
            GameObject go = (GameObject)blackboard["target"];
            agent.SetDestination(go.transform.position);
            float distToTarget = Vector3.Distance(go.transform.position, agent.transform.position);

            if (distToTarget < 2)
            {
                return BTNode.STATUS.SUCCESS;
            }
            return BTNode.STATUS.RUNNING;

        }

        BTNode.STATUS stayatTarget()
        {
            GameObject go = (GameObject)blackboard["target"];
            
            float distToTarget = Vector3.Distance(go.transform.position, agent.transform.position);
            
            if (distToTarget > 2)
            {
                return BTNode.STATUS.FAIL;
            } 

            return BTNode.STATUS.RUNNING;

        }
        
        private void Update()
        {
            
           
            
                treeStatus = behaviourTree.tick(blackboard);
                Debug.Log(treeStatus);
            
        }
    }
