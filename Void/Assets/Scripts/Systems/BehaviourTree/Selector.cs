public class Selector : BTComposite
{
    public Selector(string ID) : base(ID)
    {
        nodeName = ID;
    }

    public override STATUS tick(Blackboard blackboard)
    {
        STATUS childStatus = children[currentChildIndex].tick(blackboard);
        if (childStatus == STATUS.RUNNING) return STATUS.RUNNING;
        else if (childStatus == STATUS.FAIL)
        {
            currentChildIndex++;
        }
        else if (childStatus == STATUS.SUCCESS)
        {
            return STATUS.SUCCESS;
        }
        return childStatus;
    }
}