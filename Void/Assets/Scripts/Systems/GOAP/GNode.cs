using System;
using System.Collections.Generic;
using UnityEngine;

public class GNode : BTNode
{
    private GPlanner planner;
    private List<GAction> availableActions;
    private Func<Blackboard, bool> goalFunction;
    private GPlan currentPlan;
    
    public GNode(string ID, Func<Blackboard, bool> goalFunction, List<GAction> actions) : base(ID)
    {
        this.goalFunction = goalFunction;
        availableActions = actions;
        planner = new GPlanner();
    }

    public override STATUS tick(ref Blackboard blackboard)
    {
        // No plan, try to build one
        if (currentPlan == null)
        {
            if (goalFunction(blackboard)) return STATUS.SUCCESS;
            var newState = new Blackboard(blackboard);
            currentPlan = planner.Plan(newState, availableActions, goalFunction);
            //blackboard = currentPlan.blackboard;
            if (currentPlan == null)
                return STATUS.FAIL;
            currentPlan.ProgressPlan(ref blackboard);
        }

        GAction current = currentPlan.Peek();
        if (!current.UpdateAction(blackboard))
        {
            currentPlan = null;
            return STATUS.FAIL;
        }

        if (current.IsActionDone(blackboard))
        {
            currentPlan.ProgressPlan(ref blackboard);
        }

        if (currentPlan.IsComplete())
        {
            currentPlan = null;
            return STATUS.SUCCESS;   
        }

        return STATUS.RUNNING;

    }

}