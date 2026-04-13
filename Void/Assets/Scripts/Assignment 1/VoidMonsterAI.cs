using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assignment_2;
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
        [SerializeField] private LayerMask collisionLayerMask;
        
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

        private GNode movementPlanner;

        private void Awake()
        {
            basicAttack = GetComponent<BasicAttack>();
            
            health = GetComponent<Health>();
            health.OnCurrentHealthChanged += HealthOnOnCurrentHealthChanged;
            health.OnDeathStateChanged += HealthOnOnDeathStateChanged;
            animationController = GetComponent<AnimationController>();
            navMeshMovement = GetComponent<NavMeshMovement>();
            
            blackboard = new Blackboard
            {
                ["Transform"] = transform,
                ["Could Die"] = true,
                ["MovementController"] = navMeshMovement,
                ["Attack"] = basicAttack,
                ["Position"] = transform.position,
                ["Health"] = health,
                ["Time"] = 0f,
                ["Time At Last Attack"] = 0f,
                ["Time At Last Speedster Mutation"] = 0f,
                ["Movement Speed"] = navMeshMovement.PlayerStats.SprintSpeed.Value,
                ["Is At Target And Can Attack"]  = false,
                ["Check If Could Die"] = false,
            };

            behaviourTree = new BTRoot("Root");
            var mainSelector = new Selector("Retreat Vs Attack Selector");
            var safePointSequence = new Sequence("Safe Point Sequence");
            var isBelowHealth = new IsBelowHealth("Is Below Health", 100, 1f);
            var selectClosestSafePoint = new SelectClosestSafePoint("Select Closest Safe Point");
            var retreatSelector = new Selector("Retreat Selector");
            var waitSequence = new Sequence("Wait Sequence");
            var isSafe = new IsSafe("Is Safe", 50f, targetLayerMask, collisionLayerMask);
            var wait = new Wait("Wait");
            var retreatPlanner = new GNode("Retreat Planner", RetreatGoalFunction, new List<GAction>()
            {
                new GUseMutation("Use Speedster Mutation", blackboard, "Speedster", SpeedsterEffect),
                new GUseMutation("Use Armored Skin Mutation", blackboard, "Armored Skin", ArmoredSkinEffect),
                new GCheckIfCouldDie("Check If Could Die", 2f),
                new GMoveTo("Move to Safe Point", "SafePoint", secondsPerTick, desiredStopDistanceFromTarget),
            });
            
            var searchSequence = new Sequence("Search Sequence");
            var findReachableSurvivors = new FindReachableSurvivors("Find Reachable Survivors", targetSearchRadius, targetLayerMask);
            var selectClosestTarget = new SelectClosestSurvivor("Select Closest Target");
            var movementSelector = new Selector("Movement Selector");
            var attackSequence = new Sequence("Attack Sequence");
            var isTargetInRange = new IsTargetInRange("Is Target in Range", desiredTargetRange);
            var attackTarget = new AttackTarget("Attack Target");
            
            movementPlanner = new GNode("Movement Planner", MovementGoalFunction, new List<GAction>()
            {
                new GUseMutation("Use Speedster Mutation", blackboard, "Speedster", SpeedsterEffect),
                new GCheckIfCouldDie("Check If Could Die", 2f),
                new GUseMutation("Use Armored Skin Mutation", blackboard, "Armored Skin", ArmoredSkinEffect),
                new GMoveTo("Move to Target", "Target", secondsPerTick, desiredStopDistanceFromTarget),
            });
            
            /*var attackPlanner = new GNode("Attack Planner", GoalFunction,
                new List<GAction>()
                {
                    //new GMoveToTarget("Move To Target", secondsPerTick, desiredStopDistanceFromTarget),
                    new GAttackTarget("Attack Target", desiredTargetRange),
                    //new GWait("Wait", 0.5f, secondsPerTick),
                    new GEnsureAtTargetAndCanAttack("Ensure At Target And Can Attack", secondsPerTick, desiredStopDistanceFromTarget, basicAttack.UseDuration),
                    //new GWaitFor("Wait for Attack", "Time At Last Attack", 1.8f, secondsPerTick),
                    //new GWaitFor("Wait for Speedster Mutation", "Time At Last Speedster Mutation", 12.1f, secondsPerTick),
                    //new GWaitFor("Wait for Armored Skin Mutation", "Time At Last Armored Skin Mutation", 12.1f, secondsPerTick),
                    new GUseMutation("Use Speedster Mutation", blackboard, "Speedster", SpeedsterEffect),
                    new GUseMutation("Use Armored Skin Mutation", blackboard, "Armored Skin", ArmoredSkinEffect)
                });*/
            
            //var moveToTarget = new MoveToTarget("Move To Target", desiredStopDistanceFromTarget);

            behaviourTree.children.Add(mainSelector);
            //behaviourTree.children.Add(searchSequence);
            
            mainSelector.children.Add(safePointSequence);
            safePointSequence.children.Add(isBelowHealth);
            safePointSequence.children.Add(selectClosestSafePoint);
            safePointSequence.children.Add(retreatSelector);
            retreatSelector.children.Add(waitSequence);
            waitSequence.children.Add(isSafe);
            waitSequence.children.Add(wait);
            
            retreatSelector.children.Add(retreatPlanner);
            
            mainSelector.children.Add(searchSequence);
            searchSequence.children.Add(findReachableSurvivors);
            searchSequence.children.Add(selectClosestTarget);
            searchSequence.children.Add(movementSelector);
            
            //movementSelector.children.Add(attackPlanner);
            
            movementSelector.children.Add(attackSequence);
            attackSequence.children.Add(isTargetInRange);
            attackSequence.children.Add(attackTarget);
            
            //movementSelector.children.Add(moveToTarget);
            movementSelector.children.Add(movementPlanner);
        }

        private void HealthOnOnCurrentHealthChanged(float previousValue, float newValue, float maxValue)
        {
            if (newValue < previousValue) movementPlanner.Interrupt();
            Debug.Log(newValue);
        }

        private Blackboard SpeedsterEffect(Blackboard newBlackboard)
        {
            newBlackboard["Movement Speed"] = (float)newBlackboard["Movement Speed"] * 2f;
            return newBlackboard;
        }
        
        private Blackboard ArmoredSkinEffect(Blackboard newBlackboard)
        {
            newBlackboard["Movement Speed"] = (float)newBlackboard["Movement Speed"] / 2f;
            newBlackboard["Could Die"] = false;
            return newBlackboard;
        }

        private bool GoalFunction(Blackboard blackboard)
        {
            var targetHealth = (float)blackboard["Target Health Value"];
            return targetHealth <= 0;
        }
        
        private bool RetreatGoalFunction(Blackboard blackboard)
        {
            var distance = Vector3.Distance((Vector3)blackboard["Position"], ((Transform)blackboard["SafePoint"]).position);
            return distance <= desiredStopDistanceFromTarget;
        }

        private bool RetreatResetFunction(Blackboard blackboard)
        {
            return true;
        }
        
        private bool MovementGoalFunction(Blackboard blackboard)
        {
            var distance = Vector3.Distance((Vector3)blackboard["Position"], ((Transform)blackboard["Target"]).position);
            return distance <= desiredStopDistanceFromTarget;
        }
        
        private bool MovementResetFunction(Blackboard blackboard)
        {
            return true;
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
                blackboard["Movement Speed"] = navMeshMovement.PlayerStats.SprintSpeed.Value;
                treeStatus = behaviourTree.tick(ref blackboard);
                yield return new WaitForSeconds(secondsPerTick);   
            }
        }
    }
}
