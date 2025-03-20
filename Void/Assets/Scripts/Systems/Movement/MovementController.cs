using UnityEngine;

public abstract class MovementController : MonoBehaviour
{
    private NetworkMovementController networkMovementController;
    private bool isMovementDisabled;
    private bool isInputDisabled;

    public NetworkMovementController NetworkMovementController => networkMovementController;
    public bool IsMovementDisabled => isMovementDisabled;
    public bool IsInputDisabled => isInputDisabled;

    public void RequestSetMovementDisabled(bool value) => networkMovementController.SetMovementDisabledServerRpc(value);
    public void RequestSetInputDisabled(bool value) => networkMovementController.SetInputDisabledServerRpc(value);
    public void RequestSetVelocity(Vector3 velocity) => networkMovementController.SetVelocityServerRpc(velocity);
    public void RequestSetRotation(Quaternion rotation) => networkMovementController.SetRotationServerRpc(rotation);
    public void RequestApplyForce(Vector3 force, ForceMode forceMode) => networkMovementController.ApplyForceServerRpc(force, forceMode);
    public void RequestTeleport(Vector3 location) => networkMovementController.TeleportServerRpc(location);

    protected virtual void Awake()
    {
        networkMovementController = GetComponent<NetworkMovementController>();
    }

    public virtual void SetMovementDisabled(bool isMovementDisabled)
    {
        this.isMovementDisabled = isMovementDisabled;
    }

    public void SetInputDisabled(bool isInputDisabled)
    {
        this.isInputDisabled = isInputDisabled;
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
}