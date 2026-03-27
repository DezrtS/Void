
using UnityEngine;
using System.Collections.Generic;

public abstract class GAction
{
	public string actionName;
	
	public GAction(string actionName) 
	{
		this.actionName = actionName;
	}

	protected GAction() { }

	public virtual float Cost(Blackboard blackboard)
	{
		return 1f;
	}
	
	public abstract bool CanHappen(Blackboard blackboard);
	
	public abstract Blackboard OnCompletion(Blackboard blackboard);
	
	public virtual void BeginAction(Blackboard blackboard) { }

	public abstract bool UpdateAction(Blackboard blackboard);
	
	public virtual void EndAction(Blackboard blackboard) { }

	public abstract bool IsActionDone(Blackboard blackboard);
}