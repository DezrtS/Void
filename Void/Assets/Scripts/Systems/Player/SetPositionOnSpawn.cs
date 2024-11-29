using Unity.Netcode;
using UnityEngine;

public class SetPositionOnSpawn : NetworkBehaviour
{
    [SerializeField] private bool isMonster;

    private void Awake()
    {
        if (!IsOwner) return;

        if (isMonster)
        {
            transform.position = GridMapManager.Instance.GetMonsterSpawnPosition();
        }
        else
        {
            transform.position = GridMapManager.Instance.GetElevatorRoomPosition();
        }
    }
}