using Unity.Netcode;
using UnityEngine;

public class NetworkElevatorManager : NetworkBehaviour
{
    private ElevatorManager elevatorManager;
    private readonly NetworkVariable<bool> isReadyToLeave = new();
    private readonly NetworkVariable<bool> isAllSurvivorsInElevator = new();

    private void Awake()
    {
        elevatorManager = GetComponent<ElevatorManager>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isReadyToLeave.OnValueChanged += OnReadyToLeaveStateChanged;
        isAllSurvivorsInElevator.OnValueChanged += OnAllSurvivorsInElevatorStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isReadyToLeave.OnValueChanged -= OnReadyToLeaveStateChanged;
        isAllSurvivorsInElevator.OnValueChanged -= OnAllSurvivorsInElevatorStateChanged;
    }

    private void OnReadyToLeaveStateChanged(bool oldState, bool newState)
    {
        if (oldState == newState) return;
        elevatorManager.SetReadyToLeave(newState);
    }

    private void OnAllSurvivorsInElevatorStateChanged(bool oldState, bool newState)
    {
        if (oldState == newState) return;
        elevatorManager.SetAllSurvivorsInElevator(newState);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyToLeaveStateServerRpc(bool isReady)
    {
        if (isReadyToLeave.Value == isReady) return;
        isReadyToLeave.Value = isReady;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAllSurvivorsInElevatorStateServerRpc(bool allSurvivorsInElevator)
    {
        if (isAllSurvivorsInElevator.Value == allSurvivorsInElevator) return;
        isAllSurvivorsInElevator.Value = allSurvivorsInElevator;
    }
}