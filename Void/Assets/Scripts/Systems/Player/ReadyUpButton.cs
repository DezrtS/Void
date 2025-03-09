using Unity.Netcode;
using UnityEngine;

public class ReadyUpButton : MonoBehaviour, IInteractable
{
    [SerializeField] private bool canUnready;
    bool isReady = false;

    public void Interact(GameObject gameObject)
    {
        if (isReady && !canUnready) return;
        isReady = !isReady;
        PlayerReadyManager.Instance.RequestSetPlayerReadyState(NetworkManager.Singleton.LocalClientId, isReady);
    }
}