using Unity.Netcode;
using UnityEngine;

public class NetworkElevatorManager : NetworkBehaviour
{
    private ElevatorManager elevatorManager;
    private readonly NetworkVariable<bool> isReadyToLeave = new();

    private void Awake()
    {
        elevatorManager = GetComponent<ElevatorManager>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isReadyToLeave.OnValueChanged += OnReadyToLeaveStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isReadyToLeave.OnValueChanged -= OnReadyToLeaveStateChanged;
    }

    private void OnReadyToLeaveStateChanged(bool oldState, bool newState)
    {
        if (oldState == newState) return;
        elevatorManager.SetReadyToLeave(newState);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyToLeaveStateServerRpc(bool isReady)
    {
        if (isReadyToLeave.Value == isReady) return;
        isReadyToLeave.Value = isReady;
    }
}