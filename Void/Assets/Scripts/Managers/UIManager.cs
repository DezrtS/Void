using System;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private TutorialData defaultTutorialData;

    [SerializeField] private GameObject loadingUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsUI;

    [SerializeField] private GameObject deathUI;

    [SerializeField] private GameObject voidMonsterUI;
    [SerializeField] private GameObject survivorUI;
    [SerializeField] private GameObject spectatorUI;

    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private TextMeshProUGUI interactableText;
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI gameTimerText;

    public static event Action<GameObject> OnSetupUI;
    public static event Action<bool> OnPause;

    private bool paused;
    private float gameTimer;

    public TextMeshProUGUI TaskText => taskText;

    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void Awake()
    {
        loadingUI.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (gameTimer > 0)
        {
            gameTimer -= Time.deltaTime;
            gameTimerText.text = ((int)gameTimer).ToString();
        }
    }

    public void SetupUI(GameManager.PlayerRole playerRole, GameObject player)
    {
        EnableUI(playerRole);
        player.GetComponent<Health>().OnDeathStateChanged += OnDeathStateChanged;
        OnSetupUI?.Invoke(player);
    }

    public void EnableUI(GameManager.PlayerRole playerRole)
    {
        switch (playerRole)
        {
            case GameManager.PlayerRole.Survivor:
                voidMonsterUI.SetActive(false);
                survivorUI.SetActive(true);
                spectatorUI.SetActive(false);
                break;
            case GameManager.PlayerRole.Monster:
                voidMonsterUI.SetActive(true);
                survivorUI.SetActive(false);
                spectatorUI.SetActive(false);
                break;
            case GameManager.PlayerRole.Spectator:
                voidMonsterUI.SetActive(false);
                survivorUI.SetActive(false);
                spectatorUI.SetActive(true);
                break;
        }
    }

    public void SetInteractableText(InteractableData interactableData)
    {
        if (interactableData == null) return;
        interactableText.text = interactableData.Description;
    }

    public void ResetInteractableText()
    {
        interactableText.text = string.Empty;
    }

    public void SetTutorialText(TutorialData tutorialData)
    {
        if (tutorialData == null) return;
        tutorialText.text = tutorialData.Description;
    }

    public void ResetTutorialText()
    {
        tutorialText.text = defaultTutorialData.Description;
    }

    public void SetGameTimer(float time)
    {
        gameTimer = time;
    }

    public void PauseUnpauseGame()
    {
        Debug.Log("PAUSING GAME");
        PauseGame(!paused);
    }

    public void PauseGame(bool paused)
    {
        Debug.Log($"PAUSING GAME: {paused}");
        this.paused = paused;
        OnPause?.Invoke(paused);
        pauseMenuUI.SetActive(paused);
    }

    public void OpenSettings(bool open)
    {
        settingsUI.SetActive(open);
    }

    public void ShowDeathScreen(bool show)
    {
        deathUI.SetActive(show);
    }

    private void OnDeathStateChanged(Health health, bool isDead)
    {
        ShowDeathScreen(isDead);
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        gameStateText.text = gameState.ToString();
        if (gameState == GameManager.GameState.WaitingToStart)
        {
            loadingUI.SetActive(false);
        }
    }

    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
}