using System.Collections;
using UnityEngine;

/// <summary>
/// Contains a list of actions
/// </summary>
    public class GPlan
    {
        // Indicates whether action should be considered or not.
        bool isValid()
        {
            return true;
        }

        //This is a function so it handles situational costs, when the world
        // state is considered when calculating the cost. 
        int get_cost(Blackboard blackboard)
        {
            return 1000;
        }
    }
