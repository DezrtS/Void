using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ROOT node for all composites and leaves. CAN ONLY HAVE ONE CHILD!
/// </summary>
/// <param name="Children">ROOT's children, or child to be precise either leaf or composite.</param>
/// <param name="currentChildIndex">Index of the current child</param>

public class BTRoot : BTNode
{
    public List<BTNode> children = new List<BTNode>();
    public int currentChildIndex = 0;

    public BTRoot(string ID) : base(ID)
    {
        nodeName = "Root";
        
    }


    public override STATUS tick(Blackboard blackboard)
    {
        if (children.Count != 1)
        {
            Debug.LogWarning("WARNING: ONLY ONE CHILD ALLOWED"+ children.Count);
        }
        
        STATUS childStatus = children[currentChildIndex].tick(blackboard);
        
        return childStatus;
    }
}