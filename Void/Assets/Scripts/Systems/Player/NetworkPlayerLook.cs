using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerLook : NetworkBehaviour
{
    [SerializeField] private bool spawnFirstPersonCameraOnSpawn;
    [SerializeField] private bool lockCameraOnCameraSpawn;

    private PlayerLook playerLook;
    private readonly NetworkVariable<float> xRotation = new();

    private void Awake()
    {
        playerLook = GetComponent<PlayerLook>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OnPlayerLookSpawn();
    }

    private void OnPlayerLookSpawn()
    {
        xRotation.OnValueChanged += OnXRotationChanged;
        if (IsOwner)
        {
            playerLook.AssignControls();
            if (spawnFirstPersonCameraOnSpawn) playerLook.SpawnFirstPersonCamera();
            
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        OnPlayerLookDespawn();
    }

    private void OnPlayerLookDespawn()
    {
        xRotation.OnValueChanged -= OnXRotationChanged;
    }

    private void OnXRotationChanged(float oldValue, float newValue)
    {
        if (oldValue == newValue) return;
        
    }

    private void OnCameraLockChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (newValue) playerLook.LockCamera(true);
        else playerLook.LockCamera(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void LockCameraServerRpc(bool value)
    {
        LockCameraClientRpc(value);
    }

    [ClientRpc(RequireOwnership = false)]
    public void LockCameraClientRpc(bool value)
    {
        playerLook.LockCamera(value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetXRotationServerRpc(float value)
    {
        xRotation.Value = value;
    }
}