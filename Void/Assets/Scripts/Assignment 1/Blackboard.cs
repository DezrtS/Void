using System.Collections.Generic;

namespace Assignment_1
{
    public class Blackboard
    {
        private Blackboard parent;
        private readonly Dictionary<string, object> data = new();

        public bool Get<T>(string key, out T returnValue)
        {
            if (data.ContainsKey(key))
            {
                if (data[key] is T)
                {
                    returnValue = (T)data[key];
                    return true;
                }
            }
            
            returnValue = default;
            return false;
        }
        
        public T Get<T>(string key)
        {
            if (!data.ContainsKey(key)) return default;
            
            if (data[key] is T)
            {
                return (T)data[key];
            }

            return default;
        }
        
        public void Set(string key, object value)
        {
            data[key] = value;
        }
    }
}
