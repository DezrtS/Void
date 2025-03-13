using Unity.Netcode;
using UnityEngine;

public class ReadyUpButton : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractableData interactableData;
    [SerializeField] private bool canUnready;
    bool isReady = false;

    public InteractableData GetInteractableData()
    {
        return interactableData;
    } 

    public void Interact(GameObject gameObject)
    {
        if (isReady && !canUnready) return;
        isReady = !isReady;
        PlayerReadyManager.Instance.RequestSetPlayerReadyState(NetworkManager.Singleton.LocalClientId, isReady);
    }
}