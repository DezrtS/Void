using System.Collections.Generic;
using UnityEngine;

public class GNode : BTNode
{
    private GPlanner planner;
    private HashSet<GAction> availableActions;
    private Queue<GAction> currentPlan;
    private HashSet<KeyValuePair<string,object>> currentGoal;
    private GameObject owner;
    
    public GNode(string ID, GameObject owner, HashSet<KeyValuePair<string,object>> goal, HashSet<GAction> actions) : base(ID)
    {
        this.owner = owner;
        this.currentGoal = goal;
        this.availableActions = actions;
        this.currentPlan = new Queue<GAction>();
        this.planner = new GPlanner();
    }

    public override STATUS tick(Blackboard blackboard)
    {
        // No plan, try to build one
        if (currentPlan.Count == 0)
        {
            var worldState = (HashSet<KeyValuePair<string, object>>)blackboard["worldState"];
            currentPlan = planner.plan(owner, availableActions, worldState, currentGoal);

            if (currentPlan == null || currentPlan.Count == 0)
                return STATUS.FAIL;

        }

        GAction current = currentPlan.Peek();

        if (!current.checkProceduralPrecondition((owner)))
        {
            currentPlan.Clear();
            return STATUS.FAIL;
        }

        current.perform(owner);

        if (current.isDone())
        {
            currentPlan.Dequeue();

        }

        if (currentPlan.Count == 0)
            return STATUS.SUCCESS;

        return STATUS.RUNNING;

    }

}