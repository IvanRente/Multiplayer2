using UnityEngine;
using UnityEngine.UI;
using FishNet.Managing;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button refreshButton;

    private void Start()
    {
        if (!lobbyManager) lobbyManager = LobbyManager.Instance;

        joinButton.onClick.AddListener(OnJoinClicked);
        refreshButton.onClick.AddListener(OnRefreshClicked);
    }

    void OnJoinClicked()
    {
        string playerName = playerNameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName)) return;

        lobbyManager.ServerAddPlayer(playerName);
    }

    void OnRefreshClicked()
    {
        lobbyManager.RefreshLobbyUI();
    }
}