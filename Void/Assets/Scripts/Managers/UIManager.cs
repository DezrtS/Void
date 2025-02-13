using System;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsUI;

    [SerializeField] private GameObject voidMonsterUI;
    [SerializeField] private GameObject survivorUI;
    [SerializeField] private GameObject spectatorUI;

    [SerializeField] private TextMeshProUGUI taskText;

    public static event Action<GameObject> OnSetupUI;
    public static event Action<bool> OnPause;

    private bool paused;
    public TextMeshProUGUI TaskText => taskText;

    public void SetupUI(GameManager.PlayerRole playerRole, GameObject player)
    {
        EnableUI(playerRole);
        OnSetupUI?.Invoke(player);
    }

    public void EnableUI(GameManager.PlayerRole playerRole)
    {
        switch (playerRole)
        {
            case GameManager.PlayerRole.None:
                voidMonsterUI.SetActive(false);
                survivorUI.SetActive(false);
                spectatorUI.SetActive(false);
                break;
            case GameManager.PlayerRole.Monster:
                voidMonsterUI.SetActive(true);
                survivorUI.SetActive(false);
                spectatorUI.SetActive(false);
                break;
            case GameManager.PlayerRole.Survivor:
                voidMonsterUI.SetActive(false);
                survivorUI.SetActive(true);
                spectatorUI.SetActive(false);
                break;
            case GameManager.PlayerRole.Spectator:
                voidMonsterUI.SetActive(false);
                survivorUI.SetActive(false);
                spectatorUI.SetActive(true);
                break;
        }
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

    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
}