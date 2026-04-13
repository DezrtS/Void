
using System;

/// <summary>
/// Selector, runs a child if it fails, runs the next child, until one succeeds, or they all fail.
/// </summary>
public class Selector : BTComposite
{
    public Selector(string ID) : base(ID)
    {
        nodeName = ID;
    }

    public override STATUS tick(ref Blackboard blackboard)
    {
        STATUS childStatus = children[currentChildIndex].tick(ref blackboard);
        //UIManager.Instance.LogConsole.AddLog($"{nodeName} - {children[currentChildIndex].nodeName}");
        
        switch (childStatus)
        {
            case STATUS.RUNNING:
                return STATUS.RUNNING;
            case STATUS.FAIL:
            {
                currentChildIndex++;
                if (currentChildIndex >= children.Count)
                {
                    currentChildIndex = 0;
                }

                break;
            }
            case STATUS.SUCCESS:
                currentChildIndex = 0;
                return STATUS.SUCCESS;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return childStatus;
    }
}