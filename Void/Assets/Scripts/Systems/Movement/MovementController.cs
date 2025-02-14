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

    protected static Vector3 HandleMovement(Vector3 move, float maxSpeed, float acceleration, float timeToAccelerate, float timeToDeaccelerate, Vector3 currentVelocity)
    {
        Vector3 targetVelocity = move.normalized * maxSpeed;
        float targetSpeed = targetVelocity.magnitude;

        Vector3 velocityDifference = targetVelocity - currentVelocity;
        velocityDifference.y = 0;
        Vector3 differenceDirection = velocityDifference.normalized;
        float accelerationIncrement;

        if (currentVelocity.magnitude <= targetSpeed)
        {
            accelerationIncrement = GetAcceleration(acceleration, timeToAccelerate) * Time.deltaTime;
        }
        else
        {
            accelerationIncrement = GetAcceleration(acceleration, timeToDeaccelerate) * Time.deltaTime;
        }

        if (velocityDifference.magnitude < accelerationIncrement)
        {
            return velocityDifference; 
        }
        else
        {
            return differenceDirection * accelerationIncrement;
        }
    }

    public static Vector3 WorldToLocalVelocity(Vector3 worldVelocity, Quaternion rotation)
    {
        Quaternion inverseRotation = Quaternion.Inverse(rotation);

        Vector3 localVelocity = inverseRotation * worldVelocity;
        return localVelocity;
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