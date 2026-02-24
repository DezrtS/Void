using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Base for the composites: Selector, Sequence and Parallel
/// </summary>
/// <param name="Children">Composite's children, either leaves or composites.</param>
/// <param name="currentChildIndex">Index of the current child</param>
public class BTComposite: BTNode
{
    public List<BTNode> children =  new List<BTNode>();
    public int currentChildIndex = 0;
    public BTComposite(string ID) : base(ID)
    {
        nodeName = ID;
        
    }
    
    
}
