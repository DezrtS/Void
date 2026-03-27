using System.Collections.Generic;
using System.Linq;

namespace Assignment_2
{
    public class GUseSpeedsterMutation : GAction
    {
        public GUseSpeedsterMutation(string ID) : base(ID) { }
        
        public override bool CanHappen(Blackboard blackboard)
        {
            if (blackboard["Mutations"] is not List<Mutation> mutations || blackboard["Time"] is not float time || blackboard["Time At Last Speedster Mutation"] is not float timeAtLastSpeedsterMutation)
                return false;

            Mutation speedsterMutation = null;
            foreach (var mutation in mutations.Where(mutation => mutation.MutationData.DisplayName == "Speedster"))
            {
                speedsterMutation = mutation;
            }

            if (!speedsterMutation) return false;
            return time - timeAtLastSpeedsterMutation >= speedsterMutation.MutationData.Cooldown;
        }

        public override Blackboard OnCompletion(Blackboard blackboard)
        {
            blackboard["Movement Speed"] = (float)blackboard["Movement Speed"] * 2f;
            blackboard["Time At Last Speedster Mutation"] = blackboard["Time"];
            return blackboard;
        }

        public override void BeginAction(Blackboard blackboard)
        {
            var mutations = (List<Mutation>)blackboard["Mutations"];

            foreach (var mutation in mutations)
            {
                if (mutation.MutationData.DisplayName == "Speedster")
                {
                    mutation.RequestUse();
                    mutation.RequestStopUsing();
                }
            }
            
            blackboard["Time At Last Speedster Mutation"] = blackboard["Time"];
        }

        public override bool UpdateAction(Blackboard blackboard)
        {
            return true;
        }

        public override bool IsActionDone(Blackboard blackboard)
        {
            return true;
        }
    }
}