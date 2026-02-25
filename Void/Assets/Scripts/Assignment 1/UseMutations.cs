using System.Collections.Generic;
using System.Linq;

namespace Assignment_1
{
    public class UseMutations : BTNode
    {
        public UseMutations(string ID) : base(ID) {}

        public override STATUS tick(Blackboard blackboard)
        {
            if (blackboard["Mutations"] is not List<Mutation> mutations) return STATUS.FAIL;

            foreach (var mutation in mutations.Where(mutation => mutation.CanUse()))
            {
                mutation.RequestUse();
                mutation.RequestStopUsing();
            }

            return STATUS.SUCCESS;
        }
    }
}