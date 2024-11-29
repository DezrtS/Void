using Unity.Netcode;
using UnityEngine;

public class SetPositionOnSpawn : NetworkBehaviour
{
    [SerializeField] private bool isMonster;

    private float timer = 3;


    //private void Start()
    //{
    //    if (!IsClient) return;

    //    RequestMovePlayerServerRpc(new ServerRpcParams());
    //}

    private void FixedUpdate()
    {
        if (timer > 0)
        {
            RequestMovePlayerServerRpc(new ServerRpcParams());
            timer -= Time.fixedDeltaTime;
        }
        else
        {
            Destroy(this);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestMovePlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        HandleMovePlayer();
        HandleMovePlayerClientRpc();
    }

    public void HandleMovePlayer()
    {
        transform.position = new Vector3(65, 2, 75);
        //if (isMonster)
        //{
        //    transform.position = GridMapManager.Instance.GetMonsterSpawnPosition();
        //}
        //else
        //{
        //    transform.position = GridMapManager.Instance.GetElevatorRoomPosition();
        //}
    }

    [ClientRpc]
    public void HandleMovePlayerClientRpc()
    {
        HandleMovePlayer();
    }
}