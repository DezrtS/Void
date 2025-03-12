using Unity.Netcode;
using UnityEngine;

public class ElevatorButton : MonoBehaviour, IInteractable
{
    public void Interact(GameObject gameObject)
    {
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