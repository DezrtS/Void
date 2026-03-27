using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment_2
{
    public class GUseMutation : GAction
    {
        private readonly string mutationName;
        private readonly Func<Blackboard, Blackboard> effectFunction;

        public GUseMutation(string ID, Blackboard blackboard, string mutationName, Func<Blackboard, Blackboard> effectFunction) : base(ID)
        {
            this.mutationName = mutationName;
            this.effectFunction = effectFunction;
            blackboard[$"Time At Last {mutationName} Mutation"] = float.MinValue;
        }
        
        public override bool CanHappen(Blackboard blackboard)
        {
            if (blackboard["Mutations"] is not List<Mutation> mutations || blackboard["Time"] is not float time || blackboard[$"Time At Last {mutationName} Mutation"] is not float timeAtLastMutation)
                return false;

            Mutation selectedMutation = null;
            foreach (var mutation in mutations.Where(mutation => mutation.MutationData.DisplayName == mutationName))
            {
                selectedMutation = mutation;
            }

            if (!selectedMutation) return false;
            return time - timeAtLastMutation >= selectedMutation.MutationData.Cooldown;
        }

        public override Blackboard OnCompletion(Blackboard blackboard)
        {
            effectFunction(blackboard);
            blackboard[$"Time At Last {mutationName} Mutation"] = blackboard["Time"];
            return blackboard;
        }

        public override void BeginAction(Blackboard blackboard)
        {
            var mutations = (List<Mutation>)blackboard["Mutations"];

            foreach (var mutation in mutations)
            {
                if (mutation.MutationData.DisplayName == mutationName)
                {
                    mutation.RequestUse();
                    mutation.RequestStopUsing();
                }
            }
            
            blackboard[$"Time At Last {mutationName} Mutation"] = blackboard["Time"];
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