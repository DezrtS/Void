using System.Collections.Generic;
using UnityEngine.ProBuilder;

    public class Sequence : BTComposite
    {
        public Sequence(string ID) : base(ID)
        {
            nodeName = ID;
        }

        public override STATUS tick(Blackboard blackboard)
        {
            STATUS childStatus = children[currentChildIndex].tick(blackboard);
            if (childStatus == STATUS.RUNNING) return STATUS.RUNNING;
            else if (childStatus == STATUS.FAIL)
            {
                currentChildIndex = 0;
                return childStatus;
            }

            currentChildIndex++;
            if (currentChildIndex >= children.Count)
            {
                currentChildIndex = 0;
                return STATUS.SUCCESS;
            }
            return STATUS.RUNNING;
        }
    }
