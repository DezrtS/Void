using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains a list of actions
/// </summary>
    public class GPlan
    {
        public Blackboard blackboard;
        public List<GAction> plannedActions = new List<GAction>();
        public float cost;
        
        private int actionIndex = -1;

        public void ProgressPlan()
        {
            if (actionIndex >= 0 && actionIndex < plannedActions.Count)
            {
                var previousAction = plannedActions[actionIndex];
                previousAction.EndAction(blackboard);
            }
            actionIndex++;
            if (actionIndex >= 0 && actionIndex < plannedActions.Count)
            {
                var nextAction = plannedActions[actionIndex];
                nextAction.BeginAction(blackboard);
            }
        }

        public GAction Peek()
        {
            return plannedActions[actionIndex];
        }

        public bool IsComplete()
        {
            return actionIndex >= plannedActions.Count;
        }

        public GPlan CreateCopyWithAction(GAction newAction)
        {
            GPlan newPlan = new GPlan
            {
                //Set the new cost
                cost = cost + newAction.Cost(blackboard)
            };

            //Create the new world state
            Blackboard newState = new Blackboard(blackboard);
            newAction.OnCompletion(newState);
            newPlan.blackboard = newState;

            //Copy all the previous actions, and add the new one
            newPlan.plannedActions.AddRange(plannedActions);
            newPlan.plannedActions.Add(newAction);

            return newPlan;
        }
    }
