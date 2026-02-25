using System.Collections.Generic;
using System.Linq;

namespace Assignment_1
{
    public class HasRechargedMutation : BTNode
    {
        public HasRechargedMutation(string ID) : base(ID) {}

        public override STATUS tick(Blackboard blackboard)
        {
            if (blackboard["Mutations"] is not List<Mutation> mutations) return STATUS.FAIL;
            return mutations.Any(mutation => mutation.CanUse()) ? STATUS.SUCCESS : STATUS.FAIL;
        }
    }
}