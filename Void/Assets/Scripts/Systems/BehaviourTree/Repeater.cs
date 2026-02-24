public class Repeater : BTDecorator
{
    public int times;
    private int counter;
    public Repeater(string ID, BTNode child, int times) : base(ID, child)
    {
        nodeName = ID;
        this.child = child;
        this.times = times;
    }


    public override STATUS tick(Blackboard blackboard)
    {
        STATUS childStatus = child.tick(blackboard);
        if (childStatus == STATUS.SUCCESS && counter < times)
        {
            counter++;
        }
        else if (childStatus == STATUS.FAIL) return STATUS.FAIL;
        else
        {
            return STATUS.SUCCESS;
        }
        return childStatus;
    }
}