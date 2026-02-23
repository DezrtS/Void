using System.Collections.Generic;
using UnityEngine;
    
public class BTComposite: BTNode
{
    public List<BTNode> children =  new List<BTNode>();
    public int currentChildIndex = 0;
    public BTComposite(string ID) : base(ID)
    {
        nodeName = ID;
        
    }
    
    
}
