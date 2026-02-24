
    public class BTDecorator : BTNode
    {
        public BTNode child;
        
        public BTDecorator(string ID, BTNode child) : base(ID)
        {
            nodeName = ID;
            this.child = child;
            
        }
        
    }
