using UnityEngine;

namespace Assignment_2
{
    public class GWait : GAction
    {
        private readonly float timeToWait;
        private readonly float secondsPerTick;
        private float timer;
        
        public GWait(string ID, float timeToWait, float secondsPerTick) : base(ID)
        {
            this.timeToWait = timeToWait;
            this.secondsPerTick = secondsPerTick;
        }

        public override float Cost(Blackboard blackboard)
        {
            return timeToWait * 10f;
        }

        public override bool CanHappen(Blackboard blackboard)
        {
            return true;
        }

        public override Blackboard OnCompletion(Blackboard blackboard)
        {
            blackboard["Time"] = (float)blackboard["Time"] + timeToWait;
            return blackboard;
        }

        public override void BeginAction(Blackboard blackboard)
        {
            timer = 0;
        }

        public override bool UpdateAction(Blackboard blackboard)
        {
            timer += Time.deltaTime + secondsPerTick;
            return true;
        }

        public override void EndAction(Blackboard blackboard)
        {
            blackboard["Time"] = (float)blackboard["Time"] + timeToWait;
        }

        public override bool IsActionDone(Blackboard blackboard)
        {
            return timer >= timeToWait;
        }
    }
}