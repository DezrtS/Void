using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorManager : Singleton<ElevatorManager>
{
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Trigger trigger;

    [SerializeField] private EventReference elevatorOpenSound;
    [SerializeField] private EventReference elevatorCloseSound;
    [SerializeField] private EventReference elevatorDingSound;

    [SerializeField] private EventReference elevatorMoveDownSound;
    [SerializeField] private EventReference elevatorMoveUpSound;

    private NetworkElevatorManager networkElevatorManager;
    private bool isReadyToLeave;
    private bool isAllSurvivorsInElevator;

    private List<ulong> clientsInElevator = new();
    private EventInstance elevatorMovementInstance;

    public NetworkElevatorManager NetworkElevatorManager => networkElevatorManager;
    public bool IsReadyToLeave => isReadyToLeave;
    public bool IsAllSurvivorsInElevator => isAllSurvivorsInElevator;

    public void RequestSetReadyToLeaveState(bool isReady) => networkElevatorManager.SetReadyToLeaveStateServerRpc(isReady);
    public void RequestSetAllSurvivorsInElevatorState(bool allSurvivorsInElevator) => networkElevatorManager.SetAllSurvivorsInElevatorStateServerRpc(allSurvivorsInElevator);

    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.OnGameStateChanged += OnGameStateChanged;
        TaskManager.OnAllTasksCompleted += OnAllTasksCompleted;

        trigger.OnEnter += OnPlayerEnter;
        trigger.OnExit += OnPlayerExit;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
        TaskManager.OnAllTasksCompleted -= OnAllTasksCompleted;

        trigger.OnEnter -= OnPlayerEnter;
        trigger.OnExit -= OnPlayerExit;
    }

    private void Awake()
    {
        networkElevatorManager = GetComponent<NetworkElevatorManager>();
        elevatorMovementInstance = AudioManager.CreateEventInstance(elevatorMoveDownSound);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(elevatorMovementInstance, transform, true);
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        if (gameState == GameManager.GameState.WaitingToStart)
        {
            elevatorMovementInstance.start();
        }
        else if (gameState == GameManager.GameState.GamePlaying)
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
        AudioManager.PlayOneShot(elevatorOpenSound, transform.position);
        AudioManager.PlayOneShot(elevatorDingSound, transform.position);
        elevatorMovementInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void Leave()
    {
        doorAnimator.SetBool("Open", false);
        AudioManager.PlayOneShot(elevatorCloseSound, transform.position);
        elevatorMovementInstance.start();
    }

    public void SetReadyToLeave(bool isReadyToLeave)
    {
        this.isReadyToLeave = isReadyToLeave;
        Debug.Log($"READY TO LEAVE STATE UPDATE: {isReadyToLeave}");
    }

    public void SetAllSurvivorsInElevator(bool isAllSurvivorsInElevator)
    {
        this.isAllSurvivorsInElevator = isAllSurvivorsInElevator;
        Debug.Log($"ALL SURVIVORS IN ELEVATOR STATE UPDATE: {isAllSurvivorsInElevator}");
    }

    public bool AllSurvivorsInElevator()
    {
        Dictionary<ulong, GameManager.PlayerRole> playerRoleDictionary = GameManager.Instance.PlayerRoleDictionary;
        foreach (KeyValuePair<ulong, GameManager.PlayerRole> player in playerRoleDictionary)
        {
            if (player.Value == GameManager.PlayerRole.Survivor)
            {
                if (!clientsInElevator.Contains(player.Key)) return false;
            }
        }

        return true;
    }

    private void OnPlayerEnter(Trigger trigger, GameObject player)
    {
        if (networkElevatorManager.IsServer && player.TryGetComponent(out PlayerController playerController))
        {
            if (playerController.PlayerRole == GameManager.PlayerRole.Survivor)
            {
                trigger.AddGameObject(player);
                clientsInElevator.Add(playerController.NetworkObject.OwnerClientId);

                RequestSetAllSurvivorsInElevatorState(AllSurvivorsInElevator());
            }
        }
    }

    private void OnPlayerExit(Trigger trigger, GameObject player)
    {
        if (networkElevatorManager.IsServer && player.TryGetComponent(out PlayerController playerController))
        {
            if (playerController.PlayerRole == GameManager.PlayerRole.Survivor)
            {
                trigger.RemoveGameObject(player);
                clientsInElevator.Remove(playerController.NetworkObject.OwnerClientId);

                RequestSetAllSurvivorsInElevatorState(AllSurvivorsInElevator());
            }
        }
    }
}