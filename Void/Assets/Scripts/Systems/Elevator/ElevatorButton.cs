using FMODUnity;
using Unity.Netcode;
using UnityEngine;

public class ElevatorButton : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractableData readyUpInteractableData;
    [SerializeField] private InteractableData leaveInteractableData;

    [SerializeField] private EventReference pressSound;
    
    public InteractableData GetInteractableData()
    {
        if (GameManager.Instance.State == GameManager.GameState.WaitingToStart)
        {
            return readyUpInteractableData;
        }
        else if (GameManager.Instance.State == GameManager.GameState.GamePlaying || GameManager.Instance.State == GameManager.GameState.Panic)
        {
            if (ElevatorManager.Instance.IsReadyToLeave && ElevatorManager.Instance.IsAllSurvivorsInElevator) return leaveInteractableData;
        }

        return null;
    }

    public void Interact(GameObject gameObject)
    {
        AudioManager.PlayOneShot(pressSound, gameObject);
        if (GameManager.Instance.State == GameManager.GameState.WaitingToStart)
        {
            PlayerReadyManager.Instance.RequestSetPlayerReadyState(NetworkManager.Singleton.LocalClientId, true);
        }
        else if (GameManager.Instance.State == GameManager.GameState.GamePlaying || GameManager.Instance.State == GameManager.GameState.Panic)
        {
            if (ElevatorManager.Instance.IsReadyToLeave && ElevatorManager.Instance.IsAllSurvivorsInElevator) GameManager.Instance.RequestSetGameState(GameManager.GameState.GameOver);
        }
    }
}