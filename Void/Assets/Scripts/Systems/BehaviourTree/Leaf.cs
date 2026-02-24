using UnityEngine;


/// <summary>
/// The Leaf node can have no children
/// </summary>
/// <param name="tickMthd"> The assignable method for the tree's leaves.</param>
public class Leaf : BTNode
{
    public delegate STATUS tickMTHD();

    public tickMTHD tickMthd;
    public Leaf(string ID, tickMTHD mthd) : base(ID)
    {
        nodeName = ID;
        tickMthd = mthd;
        
    }

    public override STATUS tick(Blackboard blackboard)
    {
        if (tickMthd != null)
        {
            return tickMthd();
            
        }
        Debug.Log("THIS FAILED!"+ nodeName);
        return STATUS.FAIL;
    }
}
