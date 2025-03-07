using Unity.Netcode;
using UnityEngine;

public class NetworkMovementController : NetworkBehaviour
{
    private MovementController movementController;
    private readonly NetworkVariable<bool> isMovementDisabled = new();
    private readonly NetworkVariable<bool> isInputDisabled = new();

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isMovementDisabled.OnValueChanged += OnMovementDisabledStateChanged;
        isInputDisabled.OnValueChanged += OnInputDisabledStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isMovementDisabled.OnValueChanged -= OnMovementDisabledStateChanged;
        isInputDisabled.OnValueChanged -= OnInputDisabledStateChanged;
    }

    private void OnMovementDisabledStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        movementController.SetMovementDisabled(newValue);
    }

    private void OnInputDisabledStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        movementController.SetInputDisabled(newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetMovementDisabledServerRpc(bool value)
    {
        isMovementDisabled.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetInputDisabledServerRpc(bool value)
    {
        isInputDisabled.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetVelocityServerRpc(Vector3 velocity)
    {
        SetVelocityClientRpc(velocity);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SetVelocityClientRpc(Vector3 velocity)
    {
        if (IsOwner) movementController.SetVelocity(velocity);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRotationServerRpc(Quaternion rotation)
    {
        SetRotationClientRpc(rotation);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SetRotationClientRpc(Quaternion rotation)
    {
        if (IsOwner) movementController.SetRotation(rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyForceServerRpc(Vector3 force, ForceMode forceMode)
    {
        ApplyForceClientRpc(force, forceMode);
    }

    [ClientRpc(RequireOwnership = false)]
    public void ApplyForceClientRpc(Vector3 force, ForceMode forceMode)
    {
        if (IsOwner) movementController.ApplyForce(force, forceMode);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TeleportServerRpc(Vector3 location)
    {
        TeleportClientRpc(location);
    }

    [ClientRpc(RequireOwnership = false)]
    public void TeleportClientRpc(Vector3 location)
    {
        if (IsOwner) movementController.Teleport(location);
    }
}