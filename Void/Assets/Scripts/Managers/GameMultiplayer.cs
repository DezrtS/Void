using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameMultiplayer : Singleton<GameMultiplayer>
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public static ClientRpcParams GenerateClientRpcParams(ServerRpcParams rpcParams)
    {
        ulong callingClientId = rpcParams.Receive.SenderClientId;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != callingClientId).ToArray()
            }
        };
        return clientRpcParams;
    }
}
