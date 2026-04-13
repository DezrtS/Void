namespace Assignment_1
{
    public class IsBelowHealth : BTNode
    {
        private readonly float healthAmount;
        private readonly float aheadInTime;
        
        public IsBelowHealth(string ID, float healthAmount, float aheadInTime) : base(ID)
        {
            this.healthAmount = healthAmount;
            this.aheadInTime = aheadInTime;
        }

        public override STATUS tick(ref Blackboard blackboard)
        {
            if (blackboard["Health"] is not Health health) return STATUS.FAIL;
            if (health.WillDieInTime(aheadInTime)) return STATUS.SUCCESS;
            return health.CurrentHealth < healthAmount ? STATUS.SUCCESS : STATUS.FAIL;
        }
    }
}