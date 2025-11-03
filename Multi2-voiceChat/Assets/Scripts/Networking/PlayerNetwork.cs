using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [TargetRpc]
    public void ReceiveCreatedLobby(NetworkConnection target, string lobbyId, int lobbyNetworkObjectId)
    {
        Debug.Log($"Created lobby {lobbyId} (objectId {lobbyNetworkObjectId})");

        // Automatically join it after creation
        var lobbyMgr = LobbyManager.Instance;
        if (lobbyMgr != null)
        {
            lobbyMgr.RequestJoinByNetworkId((uint)lobbyNetworkObjectId, Owner);
        }
    }

    // Receive lobby list payload (simple serialized string)
    [TargetRpc]
    public void ReceiveLobbyList(NetworkConnection target, string payload)
    {
        Debug.Log($"Lobby list: {payload}");
        var bridge = FindAnyObjectByType<LobbyUIBridge>();
        if (bridge != null)
            bridge.PopulateLobbyList(payload);
    }
    
    [Client]
    public void JoinLobbyByNetworkId(uint lobbyNetworkObjectId)
    {
        LobbyManager.Instance.RequestJoinByNetworkId(lobbyNetworkObjectId, Owner);
    }

    [TargetRpc]
    public void AcceptJoin(NetworkConnection target, string lobbyId, int lobbyNetworkObjectId)
    {
        Debug.Log($"Joined lobby {lobbyId} (objectId {lobbyNetworkObjectId})");
    }

    [TargetRpc]
    public void Notify(NetworkConnection target, string msg)
    {
        Debug.Log($"Notify: {msg}");
    }

    [TargetRpc]
    public void StartGame(NetworkConnection target, string lobbyId)
    {
        Debug.Log($"Starting game for lobby {lobbyId}");

        // Load your gameplay scene (example: "GameScene")
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}