using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using FishNet.Connection;
using TMPro;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    private readonly SyncList<LobbyPlayer> lobbyPlayers = new();

    [Header("UI")] public GameObject lobbyUIPanel;
    public GameObject playerItemPrefab;
    public Transform playerListParent;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        // Subscribe to SyncList callback so clients update when server modifies list

        // initial refresh if already items present
        RefreshLobbyUI();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    public void ClientRequestJoin(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName)) return;

        // Call the ServerRpc; ownership required by default so the RPC will use sender param
        ServerAddPlayer(playerName);
    }


    [ServerRpc(RequireOwnership = false)]
    public void ServerAddPlayer(string playerName, NetworkConnection sender = null)
    {
        if (sender == null) return;

        int connectionId = sender.ClientId;

        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            if (lobbyPlayers[i].ConnectionId == connectionId)
            {
                // Update player name if changed
                var existing = lobbyPlayers[i];
                existing.PlayerName = playerName;
                lobbyPlayers[i] = existing;
                return;
            }
        }

        var newPlayer = new LobbyPlayer { PlayerName = playerName, ConnectionId = connectionId };
        lobbyPlayers.Add(newPlayer);

        Debug.Log($"Server: Player added: {playerName} (conn {connectionId})");
    }




    public override void OnStopServer()
    {
        base.OnStopServer();
        lobbyPlayers.Clear();
    }

    private void OnLobbyPlayersChanged(SyncListOperation op, int index, LobbyPlayer oldItem, LobbyPlayer newItem)
    {
        RefreshLobbyUI();
    }

    // Rebuild UI list (client-side)
    public void RefreshLobbyUI()
    {
        if (playerListParent == null || playerItemPrefab == null) return;

        // Clear
        for (int i = playerListParent.childCount - 1; i >= 0; i--)
            Destroy(playerListParent.GetChild(i).gameObject);

        // Populate
        foreach (var p in lobbyPlayers)
        {
            var go = Instantiate(playerItemPrefab, playerListParent);
            // Try TMP first, fallback to UnityEngine.UI.Text
            var tmp = go.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = p.PlayerName;
            else
            {
                var uiText = go.GetComponentInChildren<UnityEngine.UI.Text>();
                if (uiText != null) uiText.text = p.PlayerName;
            }
        }
    }
}

[System.Serializable]
public struct LobbyPlayer
{
    public string PlayerName;
    public int ConnectionId;
}