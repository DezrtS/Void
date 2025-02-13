using Unity.Netcode;
using UnityEngine;

public abstract class MovementController : NetworkBehaviour
{
    protected bool isDisabled;
    public bool IsDisabled => isDisabled;

    protected static float GetAcceleration(float maxSpeed, float timeToReachFullSpeed)
    {
        if (timeToReachFullSpeed == 0)
        {
            return maxSpeed;
        }

        return (maxSpeed) / timeToReachFullSpeed;
    }

    public abstract Vector3 GetVelocity();
    public abstract void SetVelocity(Vector3 velocity);
    public virtual Quaternion GetRotation()
    {
        return transform.rotation;
    }

    public virtual void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public abstract void ApplyForce(Vector3 force, ForceMode forceMode);

    public virtual void Teleport(Vector3 location)
    {
        transform.position = location;
    }
}