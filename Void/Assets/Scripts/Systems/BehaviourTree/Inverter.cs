
    public class Inverter : BTDecorator
    {
        public Inverter(string ID, BTNode child) : base(ID, child)
        {
            nodeName = ID;
            this.child = child;
        }


        public override STATUS tick(Blackboard blackboard)
        {
            STATUS childStatus = child.tick(blackboard);
            if (childStatus == STATUS.SUCCESS) return STATUS.FAIL;
            else if (childStatus == STATUS.FAIL) return STATUS.SUCCESS;
            return childStatus;
        }
    }
