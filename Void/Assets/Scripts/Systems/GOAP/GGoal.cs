using System.Collections.Generic;
using UnityEngine;


public class GGoal
{

    bool IsValid()
    {
        return true;
    }
    
    int Priority()
    {
        return 1000;
    }

    public Dictionary<string, object> GetDesiredState()
    {
        return new Dictionary<string, object>();
    }
}
