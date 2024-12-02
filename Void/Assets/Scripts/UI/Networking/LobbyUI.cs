using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject createJoinButtons;
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;

    [SerializeField] private GameObject playerButtons;
    [SerializeField] private Button changeRoleButton;
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private Button readyButton;

    private bool monsterSelected = false;

    private void Awake()
    {
        createGameButton.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.StartHost();
            createJoinButtons.SetActive(false);
            playerButtons.SetActive(true);
        });

        joinGameButton.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.StartClient();
            createJoinButtons.SetActive(false);
            playerButtons.SetActive(true);
        });

        changeRoleButton.onClick.AddListener(() =>
        {
            monsterSelected = !monsterSelected;
            roleText.text = monsterSelected ? "Change To Survivor" : "Change To Monster";
            GameManager.Instance.RequestPlayerRoleServerRpc(monsterSelected ? GameManager.PlayerRole.Monster : GameManager.PlayerRole.Survivor);
        });

        readyButton.onClick.AddListener(() =>
        {
            PlayerReadyManager.Instance.RequestPlayerReadyServerRpc();
        });
    }


}
