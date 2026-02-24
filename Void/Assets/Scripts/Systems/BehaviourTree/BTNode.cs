using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base Node for all classes tree nodes.
/// </summary>
/// <param name="nodeName">node's ID</param>
/// <param name="STATUS">possible states for each node</param>
/// <param name="tick"> update for each frame</param>

public class BTNode
{
    public string nodeName;
    
    public enum STATUS { FAIL, RUNNING,SUCCESS }
    public STATUS status;
    
    public Blackboard blackboard;
    public BTNode(string ID)
    {
        nodeName = ID;
    }

    public virtual STATUS tick(Blackboard blackboard)
    {
        return status;
    }

}
