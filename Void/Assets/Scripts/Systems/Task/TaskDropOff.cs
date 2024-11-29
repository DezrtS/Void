using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskDropOff : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Task task)) {
            task.RequestCompleteTaskServerRpc(new Unity.Netcode.ServerRpcParams());
        }
    }
}
