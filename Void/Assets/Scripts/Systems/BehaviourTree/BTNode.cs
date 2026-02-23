using System;
using System.Collections.Generic;
using UnityEngine;

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
