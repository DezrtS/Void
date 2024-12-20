using System;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject pauseMenuUI;

    [SerializeField] private GameObject voidMonsterUI;
    [SerializeField] private GameObject survivorUI;
    [SerializeField] private GameObject spectatorUI;

    public static event Action<GameObject> OnSetupUI;

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

    public void PauseGame(bool paused)
    {
        pauseMenuUI.SetActive(paused);
    }
}