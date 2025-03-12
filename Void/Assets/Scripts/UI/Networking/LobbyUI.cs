using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject createJoinButtons;
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;

    [SerializeField] private TMP_InputField IPAddressInputField;

    [SerializeField] private GameObject playerButtons;
    [SerializeField] private Button changeRoleButton;
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private Button readyButton;

    private bool monsterSelected = false;

    private void Awake()
    {
        createGameButton.onClick.AddListener(() =>
        {
            string ipAddress = "127.0.0.1";
            if (IPAddressInputField.text != string.Empty) ipAddress = IPAddressInputField.text;
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, 7777);
            GameMultiplayer.StartHost();
            createJoinButtons.SetActive(false);
            playerButtons.SetActive(true);
        });

        joinGameButton.onClick.AddListener(() =>
        {
            string ipAddress = "127.0.0.1";
            if (IPAddressInputField.text != string.Empty) ipAddress = IPAddressInputField.text;
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, 7777);
            GameMultiplayer.StartClient();
            createJoinButtons.SetActive(false);
            playerButtons.SetActive(true);
        });

        changeRoleButton.onClick.AddListener(() =>
        {
            monsterSelected = !monsterSelected;
            roleText.text = monsterSelected ? "Change To Survivor" : "Change To Monster";
            GameManager.Instance.RequestSetPlayerRole(NetworkManager.Singleton.LocalClientId, monsterSelected ? GameManager.PlayerRole.Monster : GameManager.PlayerRole.Survivor);
        });

        readyButton.onClick.AddListener(() =>
        {
            PlayerReadyManager.Instance.RequestSetPlayerReadyState(NetworkManager.Singleton.LocalClientId, true);
        });
    }


}
