using System;
using System.Collections.Generic;

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
        STATUS childStatus = children[currentChildIndex].tick(blackboard);
        
        return childStatus;
    }
}