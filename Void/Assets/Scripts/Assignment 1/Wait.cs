using UnityEngine;

namespace Assignment_1
{
    public class Wait : BTNode
    {
        public Wait(string ID) : base(ID) {}

        public override STATUS tick(ref Blackboard blackboard)
        {
            //Debug.Log("Waiting");
            return STATUS.SUCCESS;
        }
    }
}