using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assignment_1
{
    public class VoidMonsterAI : MonoBehaviour
    {
        [SerializeField] private float secondsPerTick;

        [SerializeField] private float respawnDelay;
        
        [SerializeField] private List<MutationData> mutationsData;

        [SerializeField] private float desiredTargetRange;
        [SerializeField] private float desiredStopDistanceFromTarget;
        
        [SerializeField] private float targetSearchRadius;
        [SerializeField] private LayerMask targetLayerMask;
        
        private BasicAttack basicAttack;
        private List<Mutation> mutations;
        
        private Health health;
        private AnimationController animationController;
        private NavMeshMovement navMeshMovement;
        
        private BTRoot behaviourTree;
        private BTNode.STATUS treeStatus;
        private Blackboard blackboard;

        private bool endTree;
        private float respawnTimer;

        private void Awake()
        {
            basicAttack = GetComponent<BasicAttack>();
            
            health = GetComponent<Health>();
            health.OnDeathStateChanged += HealthOnOnDeathStateChanged;
            animationController = GetComponent<AnimationController>();
            navMeshMovement = GetComponent<NavMeshMovement>();
            
            blackboard = new Blackboard
            {
                ["Transform"] = transform,
                ["MovementController"] = navMeshMovement,
                ["Attack"] = basicAttack
            };

            behaviourTree = new BTRoot("Root");
            var mainSelector = new Selector("Mutations Vs Attack Selector");
            var mutationSequence = new Sequence("Mutation Sequence");
            var hasRechargedMutation = new HasRechargedMutation("Has Recharged Mutation");
            var useMutations = new UseMutations("Use Mutations");
            var searchSequence = new Sequence("Search Sequence");
            var findReachableSurvivors = new FindReachableSurvivors("Find Reachable Survivors", targetSearchRadius, targetLayerMask);
            var selectClosestTarget = new SelectClosestSurvivor("Select Closest Target");
            var movementSelector = new Selector("Movement Selector");
            var attackSequence = new Sequence("Attack Sequence");
            var isTargetInRange = new IsTargetInRange("Is Target in Range", desiredTargetRange);
            var attackTarget = new AttackTarget("Attack Target");
            var moveToTarget = new MoveToTarget("Move To Target", desiredStopDistanceFromTarget);

            behaviourTree.children.Add(mainSelector);
            
            mainSelector.children.Add(mutationSequence);
            mutationSequence.children.Add(hasRechargedMutation);
            mutationSequence.children.Add(useMutations);
            
            mainSelector.children.Add(searchSequence);
            searchSequence.children.Add(findReachableSurvivors);
            searchSequence.children.Add(selectClosestTarget);
            searchSequence.children.Add(movementSelector);
            
            movementSelector.children.Add(attackSequence);
            attackSequence.children.Add(isTargetInRange);
            attackSequence.children.Add(attackTarget);
            
            movementSelector.children.Add(moveToTarget);
        }

        private void Start()
        {
            mutations = new List<Mutation>();
            foreach (var mutation in mutationsData.Select(GameDataManager.SpawnMutation))
            {
                mutation.SetupMutation(gameObject);
                mutations.Add(mutation);
            }

            blackboard["Mutations"] = mutations;
            
            StartCoroutine(BehaviourTreeRoutine());
        }

        private void HealthOnOnDeathStateChanged(Health health, bool isDead)
        {
            navMeshMovement.RequestSetInputDisabled(isDead);
            if (isDead)
            {
                animationController.SetTrigger("Die");
                animationController.SetTrigger("DisableIK");
                respawnTimer = respawnDelay;
            }
            else
            {
                animationController.SetTrigger("Respawn");
                animationController.SetTrigger("EnableIK");
                health.RequestFullHeal();
                navMeshMovement.Teleport(SpawnManager.Instance.GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType.Monster));
            }
        }

        private void Update()
        {
            if (respawnTimer <= 0) return;
            
            var deltaTime = Time.deltaTime;
            respawnTimer -= deltaTime;
            if (respawnTimer < 0)
            {
                health.RequestRespawn();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            endTree = true;
        }

        private IEnumerator BehaviourTreeRoutine()
        {
            while (!endTree)
            {
                treeStatus = behaviourTree.tick(blackboard);
                yield return new WaitForSeconds(secondsPerTick);   
            }
        }
    }
}
