using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [TargetRpc]
    public void ReceiveCreatedLobby(NetworkConnection target, string lobbyId, int lobbyNetworkObjectId)
    {
        Debug.Log($"Created lobby {lobbyId} (objectId {lobbyNetworkObjectId})");
        // Client can store lobbyNetworkObjectId to join later via LobbyNetworkBehaviour.ServerRpc_RequestJoin
    }

    // Receive lobby list payload (simple serialized string)
    [TargetRpc]
    public void ReceiveLobbyList(NetworkConnection target, string payload)
    {
        Debug.Log($"Lobby list: {payload}");
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
        Debug.Log($"Start game for lobby {lobbyId}");
    }
}