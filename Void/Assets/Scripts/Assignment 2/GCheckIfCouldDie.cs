namespace Assignment_2
{
    public class GCheckIfCouldDie : GAction
    {
        private float aheadInTime;
        
        public GCheckIfCouldDie(string ID, float aheadInTime) : base(ID)
        {
            this.aheadInTime = aheadInTime;
        }
        
        public override float Cost(Blackboard blackboard)
        {
            var timeDif = (float)blackboard["Time"] - (float)blackboard["Time At Plan Start"];
            return timeDif + aheadInTime;
        }

        public override bool CanHappen(Blackboard blackboard)
        {
            if (blackboard["Health"] is not Health) return false;
            return blackboard["Check If Could Die"] is false;
        }

        public override Blackboard OnCompletion(Blackboard blackboard)
        {
            var timeDif = (float)blackboard["Time"] - (float)blackboard["Time At Plan Start"];
            
            var health = (Health)blackboard["Health"];
            blackboard["Could Die"] = health.WillDieInTime(timeDif + aheadInTime);
            blackboard["Check If Could Die"] = true;
            return blackboard;
        }

        public override void BeginAction(Blackboard blackboard)
        {
            base.BeginAction(blackboard);
            var health = (Health)blackboard["Health"];
            blackboard["Could Die"] = health.WillDieInTime(aheadInTime);
        }

        public override void EndAction(Blackboard blackboard)
        {
            base.EndAction(blackboard);
            blackboard["Check If Could Die"] = false;
            blackboard["Could Die"] = true;
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