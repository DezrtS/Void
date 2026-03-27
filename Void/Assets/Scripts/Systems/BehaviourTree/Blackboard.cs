using System.Collections.Generic;


/// <summary>
/// The blackboard is the dictionary of values that the tree can use for any information from to assess it's decisions
/// </summary>

    public class Blackboard
    {
        public Dictionary<string, object> values = new Dictionary<string, object>();

        public object this[string key]
        {
            get { return TryGetValue(key); }
            
            set { values[key] = value; }
        }

        public Blackboard()
        {
            
        }
        
        public Blackboard(Blackboard other)
        {
            foreach (var keyValuePair in other.values)
            {
                values[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        public void Clear()
        {
            values.Clear();
        }

        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return this.values.Remove(key);
        }

        public object TryGetValue(string key, object value = default)
        {
            object ret = default;

            if (this.values.TryGetValue(key, out ret))
            {
                return ret;
            }

            return value;
        }
        
        public bool EqualTo(Blackboard otherBlackboard)
        {
            if (otherBlackboard.values.Count != values.Count) return false;
            
            foreach (var kvp in values)
            {
                if (!otherBlackboard.values.ContainsKey(kvp.Key)) return false;

                if (!otherBlackboard.values[kvp.Key].Equals(kvp.Value)) return false;
            }

            return true;
        }
    }
