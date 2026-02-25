namespace Assignment_1
{
    public class AttackTarget : BTNode
    {
        public AttackTarget(string ID) : base(ID) {}

        public override STATUS tick(Blackboard blackboard)
        {
            if (blackboard["Attack"] is not BasicAttack basicAttack) return STATUS.FAIL;

            if (basicAttack.CanUse())
            {
                basicAttack.RequestUse();
            }
            if (basicAttack.IsAttacking) return STATUS.RUNNING;
            
            basicAttack.RequestStopUsing();
            return STATUS.SUCCESS;
        }
    }
}