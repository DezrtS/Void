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

            //TREE ROOT, SELECTOR SEQUENCES
            behaviourTree = new BTRoot("Root");
            Selector mainSelector = new Selector("mainSelector");
            Sequence goToSequence = new Sequence("goToSequence");
            Sequence idleSequence = new Sequence("idleSequence");
            blackboard["target"] = target;
            
            Leaf goToTarget = new Leaf("goToTarget",this.goToTarget);
            Leaf stayatTarget = new Leaf("stayatTarget",this.stayatTarget);
            Leaf idle = new Leaf("idle",this.idle);
            
            goToSequence.children.Add(goToTarget);
            goToSequence.children.Add(stayatTarget);
            
            idleSequence.children.Add(idle);
            
            mainSelector.children.Add(idleSequence);
            mainSelector.children.Add(goToSequence);
            
            behaviourTree.children.Add(mainSelector);
        }

        

        BTNode.STATUS goToTarget()
        {
            GameObject go = (GameObject)blackboard["target"];
            agent.SetDestination(go.transform.position);
            float distToTarget = Vector3.Distance(go.transform.position, agent.transform.position);

            if (distToTarget > 10)
            {
                return BTNode.STATUS.FAIL;
            }
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

        BTNode.STATUS idle()
        {
            GameObject go = (GameObject)blackboard["target"];
            float distToTarget = Vector3.Distance(go.transform.position, agent.transform.position);
            if (distToTarget > 10)
            {
                return BTNode.STATUS.RUNNING;
            }
            return BTNode.STATUS.FAIL;
        }
        
        private void Update()
        {
            
                treeStatus = behaviourTree.tick(blackboard);
                Debug.Log(treeStatus);
            
        }
    }
