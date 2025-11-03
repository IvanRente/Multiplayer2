using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Connection;

public class LobbyUIBridge : MonoBehaviour
{
    [SerializeField] private GameObject lobbyListUiPanel;
    [SerializeField] private GameObject lobbyButtonPrefab; // prefab with Button + Text
    [SerializeField] private Transform lobbyListContent; // parent for lobby buttons
    [SerializeField] private LobbyManager lobbyManager;


    private void Start()
    {
        if (!lobbyManager) lobbyManager = FindAnyObjectByType<LobbyManager>();
    }

    public void OnCreateLobbyButtonClicked()
    {
        Debug.Log("Enter function");
        if (LobbyManager.Instance == null)
        {
            Debug.LogError("No LobbyManager in scene!");
            return;
        }


        if (!LobbyManager.Instance.IsServerStarted)
        {
            Debug.LogWarning("Server not started yet!");
            return;
        }

        Debug.Log("Creating lobby...");

        // Call the ServerRpc (FishNet will send this to the server automatically)
        LobbyManager.Instance.CreateLobby("MyLobby", 4);
    }

    public void OnRefreshClicked()
    {
        PlayerNetwork localPlayer = FindObjectOfType<PlayerNetwork>();
        if (localPlayer == null)
            return;

        LobbyManager.Instance.RequestLobbyList(localPlayer.Owner);
    }

    public void RefreshLobbyList()
    {
        if (lobbyListContent == null || lobbyButtonPrefab == null) return;

        // Clear old buttons
        foreach (Transform child in lobbyListContent)
            Destroy(child.gameObject);

        // Ask server for lobby list
        lobbyManager.RequestLobbyList(); // sends TargetRpc back to the client
    }

    public void PopulateLobbyList(string payload)
    {
        if (string.IsNullOrEmpty(payload)) return;

        string[] lines = payload.Split(';');
        foreach (var line in lines)
        {
            string[] parts = line.Split('|');
            if (parts.Length < 6) continue;

            string lobbyId = parts[0];
            string lobbyName = parts[1];
            int currentPlayers = int.Parse(parts[2]);
            int maxPlayers = int.Parse(parts[3]);
            // int state = int.Parse(parts[4]);
            uint networkObjectId = uint.Parse(parts[5]);

            GameObject btnObj = Instantiate(lobbyButtonPrefab, lobbyListContent);
            Button btn = btnObj.GetComponent<Button>();
            Text txt = btnObj.GetComponentInChildren<Text>();
            txt.text = $"{lobbyName} ({currentPlayers}/{maxPlayers})";

            btn.onClick.AddListener(() =>
            {
                PlayerNetwork localPlayer = FindObjectOfType<PlayerNetwork>();
                if (localPlayer != null)
                    localPlayer.JoinLobbyByNetworkId(networkObjectId);
            });
        }
    }

    public void OnStartGameClicked()
    {
        Debug.Log("Start Game clicked");

        PlayerNetwork localPlayer = FindFirstObjectByType<PlayerNetwork>();
        if (localPlayer == null)
        {
            Debug.LogWarning("No local player found!");
            return;
        }

        var lobby = FindAnyObjectByType<LobbyNetworkBehaviour>();
        if (lobby == null)
        {
            Debug.LogWarning("No lobby found to start!");
            return;
        }

        Debug.Log($"Requesting start for lobby: {lobby.LobbyId.Value}");
        LobbyManager.Instance.RequestStart((uint)lobby.NetworkObject.ObjectId, localPlayer.Owner);
    }
    
    // private void OnStartGame()
    // {
    //     if (LobbyManager.Instance == null)
    //         return;
    //     
    //     LobbyManager.Instance.RequestStart();
    // }

    private PlayerNetwork localPlayer;

    public void SetLocalPlayer(PlayerNetwork player)
    {
        localPlayer = player;
    }

    // private void OnStartGameClicked()
    //     {
    //         if (localPlayer == null)
    //         {
    //             Debug.LogWarning("Local player not set!");
    //             return;
    //         }
    //
    //         var lobby = localPlayer;
    //         if (lobby == null)
    //         {
    //             Debug.LogWarning("Not in a lobby!");
    //             return;
    //         }
    //
    //         // Host requests the server to start the game
    //         LobbyManager.Instance.RequestStart(lobby.NetworkObjectId, localPlayer.Owner);
    //     }
}