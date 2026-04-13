namespace Assignment_1
{
    public class IsBelowHealth : BTNode
    {
        private readonly float healthAmount;
        
        public IsBelowHealth(string ID, float healthAmount) : base(ID)
        {
            this.healthAmount = healthAmount;
        }

        public override STATUS tick(ref Blackboard blackboard)
        {
            if (blackboard["Health"] is not Health health) return STATUS.FAIL;
            return health.CurrentHealth < healthAmount ? STATUS.SUCCESS : STATUS.FAIL;
        }
    }
}