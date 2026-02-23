using UnityEngine;

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
