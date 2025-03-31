using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public delegate void GameStateHandler(GameState gameState);
    public static event GameStateHandler OnGameStateChanged;

    public static bool IsFriendlyFireDisabled;

    public enum GameState { None, WaitingToStart, ReadyToStart, GamePlaying, Panic, GameOver }
    public enum PlayerRole { Survivor, Monster, Spectator }

    [Header("Debugging")]
    [SerializeField] private bool allowGameSlowDown;
    [SerializeField] private bool showGameTimer;

    [Header("Options")]
    [SerializeField] private bool resetPlayerPrefsOnAwake;
    [SerializeField] private bool enableFriendlyFireOnAwake;
    [SerializeField] private bool endGameOnAllSurvivorDeath;
    [SerializeField] private PlayerRole defaultPlayerRole;
    [SerializeField] private float forceReadyUpTimer;
    [SerializeField] private float startGameDelay;
    [SerializeField] private float endGameDelay;

    [SerializeField] private float gameDuration;
    [SerializeField] private float panicDuration;

    [Header("Prefabs")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject survivorPrefab;

    [SerializeField] public GameObject FirstPersonCamera;

    [Header("Sound Events")]
    [SerializeField] private List<TimedDialogueEvent> dialogueEvents;

    private NetworkGameManager networkGameManager;
    private GameState state;

    private Dictionary<ulong, PlayerRole> playerRoleDictionary;
    private List<ulong> deadClientIds;
    private float gameTimer;

    public PlayerRole DefaultPlayerRole => defaultPlayerRole;
    public NetworkGameManager NetworkGameManager => networkGameManager;
    public GameState State => state;
    public Dictionary<ulong, PlayerRole> PlayerRoleDictionary => playerRoleDictionary;

    public void RequestSetGameState(GameState gameState) => networkGameManager.SetGameStateServerRpc(gameState);
    public void RequestAddPlayerRole(ulong clientId, PlayerRole playerRole) => networkGameManager.AddPlayerRoleServerRpc(clientId, playerRole);
    public void RequestSetPlayerRole(ulong clientId, PlayerRole newPlayerRole) => networkGameManager.SetPlayerRoleServerRpc(clientId, newPlayerRole);
    public void RequestRemovePlayerRole(ulong clientId) => networkGameManager.RemovePlayerRoleServerRpc(clientId);

    public void RequestSpawnPlayers() => networkGameManager.SpawnPlayersServerRpc();
    public void RequestSpawnPlayer(ulong clientId) => networkGameManager.SpawnPlayerServerRpc(clientId);

    private void Awake()
    {
        dialogueEvents.Sort((x, y) => y.Time.CompareTo(x.Time));
        networkGameManager = GetComponent<NetworkGameManager>();
        playerRoleDictionary = new Dictionary<ulong, PlayerRole>();
        deadClientIds = new List<ulong>();
        if (resetPlayerPrefsOnAwake) PlayerPrefs.DeleteAll();
        IsFriendlyFireDisabled = !enableFriendlyFireOnAwake;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            Time.timeScale = Mathf.Max(Time.timeScale - 0.05f, 0);
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            Time.timeScale += 0.05f;
        }

        if (gameTimer > 0)
        {
            gameTimer -= deltaTime;
            if (gameTimer <= 0)
            {
                if (networkGameManager.IsServer) RequestSetGameState(GameState.GameOver);
            }
            else if (dialogueEvents.Count > 0)
            {
                if (gameTimer <= dialogueEvents[0].Time)
                {
                    if (networkGameManager.IsServer) AudioManager.RequestPlayDialogue(dialogueEvents[0].DialogueData);
                    dialogueEvents.RemoveAt(0);
                }
            }
        }
    }

    public void SetGameState(GameState gameState)
    {
        Debug.Log($"GAME STATE UPDATE: {state} to {gameState}");
        state = gameState;
        OnGameStateChanged?.Invoke(state);
        HandleState(state);
    }

    private void HandleState(GameState gameState)
    {
        switch (gameState) {
            case GameState.WaitingToStart:
                RequestSpawnPlayer(networkGameManager.NetworkManager.LocalClientId);
                UIManager.Instance.SetGameTimer(forceReadyUpTimer);
                AudioManager.ChangeMusic(FMODEventManager.Instance.ElevatorTheme);
                if (networkGameManager.IsServer) InitializeGame();
                break;
            case GameState.ReadyToStart:
                UIManager.Instance.SetGameTimer(startGameDelay);
                if (networkGameManager.IsServer) PrepareGame();
                break;
            case GameState.GamePlaying:
                gameTimer = gameDuration + panicDuration;
                UIManager.Instance.SetGameTimer(gameDuration);
                AudioManager.ChangeMusic(FMODEventManager.Instance.BackgroundTheme);
                if (networkGameManager.IsServer) StartGame();
                break;
            case GameState.Panic:
                UIManager.Instance.SetGameTimer(panicDuration);
                AudioManager.ChangeMusic(FMODEventManager.Instance.AlarmTheme);
                if (networkGameManager.IsServer) Panic();
                break;
            case GameState.GameOver:
                UIManager.Instance.SetGameTimer(endGameDelay);
                AudioManager.StopMusic(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                if (networkGameManager.IsServer) EndGame();
                break;
            default:
                break;
        }
    }

    public void RequestUpdatePlayerRole(ulong clientId)
    {
        if (!playerRoleDictionary.TryGetValue(clientId, out PlayerRole playerRole))
        {
            int role = PlayerPrefs.GetInt(clientId.ToString(), -1);

            if (role >= 0)
            {
                playerRole = (PlayerRole)role;
            }
            else
            {
                playerRole = defaultPlayerRole;
            }

            RequestAddPlayerRole(clientId, playerRole);
        }
    }

    public void SpawnPlayers()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    public void SpawnPlayer(ulong clientId)
    {
        RequestUpdatePlayerRole(clientId);
        if (playerRoleDictionary.TryGetValue(clientId, out PlayerRole playerRole))
        {
            switch (playerRole)
            {
                case PlayerRole.Monster:
                    SpawnMonster(clientId);
                    break;
                case PlayerRole.Survivor:
                    SpawnSurvivor(clientId);
                    break;
                default:
                    break;
            }
        }
    }

    private NetworkObject SpawnMonster(ulong clientId)
    {
        GameObject monsterGameObject = Instantiate(monsterPrefab, SpawnManager.Instance.GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType.Monster), Quaternion.identity);
        NetworkObject networkObject = monsterGameObject.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        return networkObject;
    }

    private NetworkObject SpawnSurvivor(ulong clientId)
    {
        GameObject survivorGameObject = Instantiate(survivorPrefab, SpawnManager.Instance.GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType.Survivor), Quaternion.identity);
        if (survivorGameObject.TryGetComponent(out Health health)) health.OnDeathStateChanged += OnSurvivorDeathStateChanged;
        NetworkObject networkObject = survivorGameObject.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        return networkObject;
    }

    private void OnSurvivorDeathStateChanged(Health health, bool isDead)
    {
        if (isDead)
        {
            deadClientIds.Add(health.NetworkHealth.OwnerClientId);
            if (endGameOnAllSurvivorDeath && AllSurvivorsDead())
            {
                RequestSetGameState(GameState.GameOver);
            }
            else
            {
                if (deadClientIds.Count == 1) AudioManager.RequestPlayOneShot(FMODEventManager.Instance.OnePlayerSlayed, transform.position);
                else if (deadClientIds.Count == 2) AudioManager.RequestPlayOneShot(FMODEventManager.Instance.TwoPlayersSlayed, transform.position);
            }
        }
        else
        {
            deadClientIds.Remove(health.NetworkHealth.OwnerClientId);
        }
    }

    public bool AllSurvivorsDead()
    {
        foreach (KeyValuePair<ulong, PlayerRole> player in playerRoleDictionary)
        {
            if (player.Value == PlayerRole.Survivor)
            {
                if (!deadClientIds.Contains(player.Key)) return false;
            }
        }

        return true;
    }

    public void OnAllPlayersReady()
    {
        PlayerReadyManager.OnAllPlayersReady -= OnAllPlayersReady;
        PlayerReadyManager.Instance.RequestSetAllPlayersReadyState(false);
        if (state == GameState.None) StartCoroutine(InitializeGameCoroutine());
        else if (state == GameState.WaitingToStart) RequestSetGameState(GameState.ReadyToStart);
        else if (state == GameState.GamePlaying || state == GameState.Panic) RequestSetGameState(GameState.GameOver);
    }

    private void OnAllTasksCompleted()
    {
        TaskManager.OnAllTasksCompleted -= OnAllTasksCompleted;
    }

    public void InitializeGame()
    {
        PlayerReadyManager.OnAllPlayersReady += OnAllPlayersReady;
        StartCoroutine(ForceReadyUpCoroutine());
    }

    public void PrepareGame()
    {
        StopAllCoroutines();
        TaskManager.Instance.RequestSpawnRandomTasks();
        TaskManager.OnAllTasksCompleted += OnAllTasksCompleted;
        StartCoroutine(StartGameCoroutine());
    }

    public void StartGame()
    {
        StartCoroutine(PanicCoroutine());
        gameTimer = gameDuration + panicDuration;
    }

    public void Panic()
    {
        //StartCoroutine(EndGameCoroutine());
    }

    public void EndGame()
    {
        StopAllCoroutines();
        StartCoroutine(RestartGameCoroutine());
    }

    public void QuitGame()
    {
        CleanUp();
        NetworkManager.Singleton.Shutdown();
        Loader.Load(Loader.Scene.LobbyScene);
    }

    public void CleanUp()
    {
        StopAllCoroutines();
        TaskManager.OnAllTasksCompleted -= OnAllTasksCompleted;
        PlayerReadyManager.OnAllPlayersReady -= OnAllPlayersReady;
    }

    private IEnumerator InitializeGameCoroutine()
    {
        yield return new WaitForSeconds(1);
        RequestSetGameState(GameState.WaitingToStart);
    }

    private IEnumerator ForceReadyUpCoroutine()
    {
        yield return new WaitForSeconds(forceReadyUpTimer);
        PlayerReadyManager.Instance.RequestSetAllPlayersReadyState(true);
    }

    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(startGameDelay);
        RequestSetGameState(GameState.GamePlaying);
    }

    private IEnumerator PanicCoroutine()
    {
        yield return new WaitForSeconds(gameDuration);
        RequestSetGameState(GameState.Panic);
    }

    private IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(panicDuration);
        RequestSetGameState(GameState.GameOver);
    }

    private IEnumerator RestartGameCoroutine()
    {
        yield return new WaitForSeconds(endGameDelay);

        Loader.LoadNetwork(Loader.Scene.GameplayScene);
    }
}