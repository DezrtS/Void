using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Plans what actions can be completed in order to fulfill a goal state.
 */
public class GPlanner
{
	public GPlan Plan(Blackboard blackboard, List<GAction> possibleActions, Func<Blackboard, bool> goalFunction)
	{
		var allPlans = new List<GPlan>
		{
			new GPlan
			{
				blackboard = blackboard,
				plannedActions = new List<GAction>(),
				cost = 0
			}
		};
		
		while (allPlans.Count > 0)
        {
            int cheapestOption = 0;

            //Performance-wise this could be improved, but it serves its purpose of finding the cheapest option
            for (int i = 0; i < allPlans.Count; ++i)
            {
                if (allPlans[i].cost < allPlans[cheapestOption].cost)
                {
                    cheapestOption = i;
                }
            }

            GPlan chosenPlan = allPlans[cheapestOption];
            allPlans.RemoveAt(cheapestOption);

            //If this plan meets the criteria, then we've found a solution! Great!
            if (goalFunction(chosenPlan.blackboard))
            {
                return chosenPlan;
            }

            //Otherwise, we append new actions to the plan and keep going
            foreach (var action in possibleActions)
            {
                //Don't both with impossible actions
                if (!action.CanHappen(chosenPlan.blackboard)) continue;

                //Test the new action to see if we've already been here
                var newPlan = chosenPlan.CreateCopyWithAction(action);
                bool planLeadsToNewResults = true;

                //Test against all other plans to see if any gave the same state
                for (int i = 0; i < allPlans.Count; ++i)
                {
                    if (allPlans[i].blackboard.EqualTo(newPlan.blackboard))
                    {
                        planLeadsToNewResults = false;

                        //Replace the more expensive plan with the cheaper plan
                        if (allPlans[i].cost > newPlan.cost)
                        {
                            allPlans[i] = newPlan;
                        }

                        continue;
                    }
                }

                if (planLeadsToNewResults)
                {
                    allPlans.Add(newPlan);
                }
            }
        }

        //If no possible plan was found, log an error - this shouldn't happen
        Debug.LogError("We don't have a plan!");
        return null;
	}
}