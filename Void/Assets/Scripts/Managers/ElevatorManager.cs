using UnityEngine;

public class ElevatorManager : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;

    private NetworkElevatorManager networkElevatorManager;
    private bool isReadyToLeave;

    public NetworkElevatorManager NetworkElevatorManager => networkElevatorManager;
    public bool IsReadyToLeave => isReadyToLeave;

    public void RequestSetReadyToLeaveState(bool isReady) => networkElevatorManager.SetReadyToLeaveStateServerRpc(isReady);

    private void Awake()
    {
        networkElevatorManager = GetComponent<NetworkElevatorManager>();
        GameManager.OnGameStateChanged += OnGameStateChanged;
        TaskManager.OnAllTasksCompleted += OnAllTasksCompleted;
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        if (gameState == GameManager.GameState.GamePlaying)
        {
            Arrive();
        }
        else if (gameState == GameManager.GameState.GameOver)
        {
            Leave();
        }
    }

    private void OnAllTasksCompleted()
    {
        TaskManager.OnAllTasksCompleted -= OnAllTasksCompleted;
        if (networkElevatorManager.IsServer) RequestSetReadyToLeaveState(true);
    }

    public void Arrive()
    {
        doorAnimator.SetBool("Open", true);
    }

    public void Leave()
    {
        doorAnimator.SetBool("Open", false);
    }

    public void SetReadyToLeave(bool isReadyToLeave)
    {
        this.isReadyToLeave = isReadyToLeave;
        Debug.Log($"READY TO LEAVE STATE UPDATE: {isReadyToLeave}");
    }
}