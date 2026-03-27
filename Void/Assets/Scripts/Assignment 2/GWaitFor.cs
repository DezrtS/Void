using UnityEngine;

namespace Assignment_2
{
    public class GWaitFor : GAction
    {
        private readonly string timeAtLastActionUseKey;
        private readonly float timeToWait;
        private readonly float secondsPerTick;

        private float waitTime;
        private float timer;
        
        public GWaitFor(string ID, string timeAtLastActionUseKey, float timeToWait, float secondsPerTick) : base(ID)
        {
            this.timeAtLastActionUseKey = timeAtLastActionUseKey;
            this.timeToWait = timeToWait;
            this.secondsPerTick = secondsPerTick;
        }

        public override float Cost(Blackboard blackboard)
        {
            //var time = (float)blackboard["Time"];
            //var timeAtLastActionUse = (float)blackboard[timeAtLastActionUseKey];
            //var timeDifference = timeToWait - (time - timeAtLastActionUse);
            return timeToWait * 10f;
        }

        public override bool CanHappen(Blackboard blackboard)
        {
            var time = (float)blackboard["Time"];
            var timeAtLastActionUse = (float)blackboard[timeAtLastActionUseKey];

            return time - timeAtLastActionUse < timeToWait;
        }

        public override Blackboard OnCompletion(Blackboard blackboard)
        {
            //var time = (float)blackboard["Time"];
            //var timeAtLastActionUse = (float)blackboard[timeAtLastActionUseKey];
            //var timeDifference = Mathf.Min(timeToWait - (time - timeAtLastActionUse));
            blackboard["Time"] = (float)blackboard["Time"] + timeToWait;
            return blackboard;
        }

        public override void BeginAction(Blackboard blackboard)
        {
            //var time = (float)blackboard["Time"];
            //var timeAtLastActionUse = (float)blackboard[timeAtLastActionUseKey];
            waitTime = timeToWait;
            timer = 0;
        }

        public override bool UpdateAction(Blackboard blackboard)
        {
            timer += Time.deltaTime + secondsPerTick;
            return true;
        }

        public override void EndAction(Blackboard blackboard)
        {
            blackboard["Time"] = (float)blackboard["Time"] + waitTime;
        }

        public override bool IsActionDone(Blackboard blackboard)
        {
            return timer >= waitTime;
        }
    }
}