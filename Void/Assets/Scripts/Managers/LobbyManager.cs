using System.Collections;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : Singleton<LobbyManager>
{
    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject creditsUI;

    [SerializeField] private float transitionDuration;

    [SerializeField] private GameObject mainMenuButtonHolder;
    [SerializeField] private GameObject lobbyButtonHolder;
    [SerializeField] private GameObject gameButtonHolder;

    [SerializeField] private TMP_InputField ipAddressInputField;

    private bool selectedSurvivor = true;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ShowMainMenu()
    {
        animator.SetTrigger("Transition");
        StartCoroutine(TransitionCoroutine(true, false, false, false));
    }

    public void ShowLobby()
    {
        animator.SetTrigger("Transition");
        StartCoroutine(TransitionCoroutine(false, true, false, false));
    }

    public void ShowGame()
    {
        animator.SetTrigger("Transition");
        StartCoroutine(TransitionCoroutine(false, false, true, false));
    }

    public void ShowCredits()
    {
        animator.SetTrigger("Transition");
        StartCoroutine(TransitionCoroutine(false, false, false, true));
    }

    public void CreateGame()
    {
        string ipAddress = "127.0.0.1";
        if (ipAddressInputField.text != string.Empty) ipAddress = ipAddressInputField.text;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, 7777);
        GameMultiplayer.StartHost();
        ShowGame();
    }

    public void JoinGame()
    {
        string ipAddress = "127.0.0.1";
        if (ipAddressInputField.text != string.Empty) ipAddress = ipAddressInputField.text;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, 7777);
        GameMultiplayer.StartClient();
        ShowGame();
    }

    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
        ShowLobby();
    }

    public void ChangeRole()
    {
        selectedSurvivor = !selectedSurvivor;
        GameManager.Instance.RequestSetPlayerRole(NetworkManager.Singleton.LocalClientId, selectedSurvivor ? GameManager.PlayerRole.Survivor : GameManager.PlayerRole.Monster);
    }

    public void ReadyUp()
    {
        PlayerReadyManager.Instance.RequestSetPlayerReadyState(NetworkManager.Singleton.LocalClientId, true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator TransitionCoroutine(bool showMainMenu, bool showLobby, bool showGame, bool showCredits)
    {
        yield return new WaitForSeconds(transitionDuration);
        UI.SetActive(showMainMenu || showLobby || showGame);
        mainMenuButtonHolder.SetActive(showMainMenu);
        lobbyButtonHolder.SetActive(showLobby);
        gameButtonHolder.SetActive(showGame);
        creditsUI.SetActive(showCredits);
    }
}
