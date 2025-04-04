using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private TutorialData defaultSurvivorTutorialData;
    [SerializeField] private TutorialData defaultMonsterTutorialData;

    [SerializeField] private TutorialData defaultSurvivorObjectiveData;
    [SerializeField] private TutorialData defaultMonsterObjectiveData;

    [SerializeField] private GameObject loadingUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private GameObject objectiveUI;

    [SerializeField] private GameObject deathUI;

    [SerializeField] private GameObject voidMonsterUI;
    [SerializeField] private GameObject survivorUI;
    [SerializeField] private GameObject spectatorUI;

    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private TextMeshProUGUI interactableText;
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI gameTimerText;

    [SerializeField] private TextMeshProUGUI dialogueNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private GameObject compassBar;
    [SerializeField] private Image cooldownCircle;

    public static event Action<GameObject> OnSetupUI;
    public static event Action<bool> OnPause;

    private TutorialData defaultTutorialData;
    private Animator animator;
    private bool paused;
    private float gameTimer;
    private bool lockTutorialText;

    private float cooldownTime;
    private float cooldownTimer;

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
        defaultTutorialData = defaultSurvivorTutorialData;
        loadingUI.SetActive(true);
        SetTutorialText(defaultTutorialData);
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        if (gameTimer > 0)
        {
            gameTimer -= deltaTime;
            gameTimerText.text = ((int)gameTimer).ToString();
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= deltaTime;
            if (cooldownTimer <= 0)
            {
                ResetCooldownTimer();
            }
            else
            {
                float percentage = cooldownTimer / cooldownTime;
                cooldownCircle.fillAmount = 1 - percentage;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            gameStateText.gameObject.SetActive(!gameStateText.gameObject.activeSelf);
            gameTimerText.gameObject.SetActive(!gameTimerText.gameObject.activeSelf);
        }
    }

    public void SetupUI(GameManager.PlayerRole playerRole, GameObject player)
    {
        EnableUI(playerRole);
        if (playerRole == GameManager.PlayerRole.Survivor)
        {
            defaultTutorialData = defaultSurvivorTutorialData;
            SetObjectiveText(defaultSurvivorObjectiveData);
        }
        else if (playerRole == GameManager.PlayerRole.Monster)
        {
            defaultTutorialData = defaultMonsterTutorialData;
            SetObjectiveText(defaultMonsterObjectiveData);
        }
        SetTutorialText(defaultTutorialData);
        lockTutorialText = true;
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

    public CompassIcon AddCompassIcon(GameObject prefab)
    {
        GameObject newIcon = Instantiate(prefab, Vector3.zero, Quaternion.identity, compassBar.transform);
        CompassIcon compassIcon = newIcon.GetComponent<CompassIcon>();
        return compassIcon;
    }

    public void SetInteractableText(InteractableData interactableData)
    {
        if (interactableData == null)
        {
            ResetInteractableText();
            return;
        }
        interactableText.text = interactableData.Description;
    }

    public void ResetInteractableText()
    {
        interactableText.text = string.Empty;
    }

    public void HideUnhideTutorialUI()
    {
        tutorialUI.SetActive(!tutorialUI.activeSelf);
        objectiveUI.SetActive(!objectiveUI.activeSelf);
    }

    public void SetTutorialText(TutorialData tutorialData)
    {
        if (tutorialData == null || lockTutorialText) return;
        tutorialText.text = tutorialData.Description;
    }

    public void ResetTutorialText()
    {
        tutorialText.text = defaultTutorialData.Description;
    }

    public void SetObjectiveText(TutorialData tutorialData)
    {
        if (tutorialData == null) return;
        objectiveText.text = tutorialData.Description;
    }

    public void SetDialogueText(DialogueData dialogueData)
    {
        ResetDialogueText();
        SetDialogue(dialogueData, 0);
    }

    private void SetDialogue(DialogueData dialogueData, int index)
    {
        if (index >= dialogueData.DialogueLines.Count)
        {
            ResetDialogueText();
            return;
        }
        dialogueNameText.text = $"[{dialogueData.DialogueNameData.SpeakerName}]";
        dialogueText.text = dialogueData.DialogueLines[index].Dialogue;
        StartCoroutine(IncrementDialogueCoroutine(dialogueData, index));
    }

    public void ResetDialogueText()
    {
        StopAllCoroutines();
        dialogueNameText.text = string.Empty;
        dialogueText.text = string.Empty;
    }

    public void SetGameTimer(float time)
    {
        gameTimer = time;
    }

    public void SetCooldownTimer(float time)
    {
        cooldownCircle.fillAmount = 0;
        cooldownCircle.gameObject.SetActive(true);
        cooldownTime = time;
        cooldownTimer = time;
    }

    public void ResetCooldownTimer()
    {
        cooldownCircle.fillAmount = 0;
        cooldownTimer = 0;
        cooldownCircle.gameObject.SetActive(false);
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

    public void TriggerFade(bool fadeIn)
    {
        if (fadeIn) animator.SetTrigger("FadeIn");
        else animator.SetTrigger("FadeOut");
    }

    public void TriggerHit()
    {
        AudioManager.PlayOneShot(FMODEventManager.Instance.HitMarkerSound);
        animator.SetTrigger("Hit");
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        gameStateText.text = gameState.ToString();
        if (gameState == GameManager.GameState.WaitingToStart)
        {
            loadingUI.SetActive(false);
            animator.SetTrigger("Tutorial");
        }
        else if (gameState == GameManager.GameState.ReadyToStart)
        {
            lockTutorialText = false;
        }
        else if (gameState == GameManager.GameState.GameOver)
        {
            TriggerFade(true);
            animator.SetTrigger("EndGame");
        }
    }

    private IEnumerator IncrementDialogueCoroutine(DialogueData dialogueData, int index)
    {
        yield return new WaitForSeconds(dialogueData.DialogueLines[index].Duration);
        index++;
        SetDialogue(dialogueData, index);
    }

    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
}