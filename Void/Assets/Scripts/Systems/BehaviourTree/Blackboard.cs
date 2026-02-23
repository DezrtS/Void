using System.Collections.Generic;



    public class Blackboard
    {
        private Dictionary<string, object> blackboard = new Dictionary<string, object>();

        public object this[string key]
        {
            get { return blackboard[key]; }
            
            set { blackboard[key] = value; }
        }

        public void Clear()
        {
            blackboard.Clear();
        }

        public bool ContainsKey(string key)
        {
            return blackboard.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return this.blackboard.Remove(key);
        }

        public object TryGetValue(string key, object value = default)
        {
            object ret = default;

            if (this.blackboard.TryGetValue(key, out ret))
            {
                return ret;
            }

            return value;
        }
    }
