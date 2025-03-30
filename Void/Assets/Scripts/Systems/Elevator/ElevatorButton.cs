using FMODUnity;
using Unity.Netcode;
using UnityEngine;

public class ElevatorButton : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractableData readyUpInteractableData;
    [SerializeField] private InteractableData waitingForOtherPlayersInteractableData;
    [SerializeField] private InteractableData allPlayersInteractableData;
    [SerializeField] private InteractableData leaveInteractableData;

    [SerializeField] private GameObject outline;
    [SerializeField] private EventReference pressSound;
    private bool isReady;

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += OnGameStateChanged;
        TaskManager.OnAllTasksCompleted += OnAllTasksCompleted;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
        TaskManager.OnAllTasksCompleted -= OnAllTasksCompleted;
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.ReadyToStart)
        {
            outline.SetActive(false);
        }
    }

    private void OnAllTasksCompleted()
    {
        outline.SetActive(true);
    }

    public InteractableData GetInteractableData()
    {
        if (GameManager.Instance.State == GameManager.GameState.WaitingToStart)
        {
            if (isReady) return waitingForOtherPlayersInteractableData;
            else return readyUpInteractableData;
        }
        else if (GameManager.Instance.State == GameManager.GameState.GamePlaying || GameManager.Instance.State == GameManager.GameState.Panic)
        {
            if (ElevatorManager.Instance.IsReadyToLeave)
            {
                if (ElevatorManager.Instance.IsAllSurvivorsInElevator)
                {
                    return leaveInteractableData;
                }
                else
                {
                    return allPlayersInteractableData;
                }
            }
        }

        return null;
    }

    public void Interact(GameObject gameObject)
    {
        AudioManager.PlayOneShot(pressSound, gameObject);
        if (GameManager.Instance.State == GameManager.GameState.WaitingToStart)
        {
            isReady = true;
            PlayerReadyManager.Instance.RequestSetPlayerReadyState(NetworkManager.Singleton.LocalClientId, true);
        }
        else if (GameManager.Instance.State == GameManager.GameState.GamePlaying || GameManager.Instance.State == GameManager.GameState.Panic)
        {
            if (ElevatorManager.Instance.IsReadyToLeave && ElevatorManager.Instance.IsAllSurvivorsInElevator) GameManager.Instance.RequestSetGameState(GameManager.GameState.GameOver);
        }
    }
}