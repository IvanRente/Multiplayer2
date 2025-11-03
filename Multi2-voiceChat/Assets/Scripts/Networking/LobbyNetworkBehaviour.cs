using System;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class LobbyNetworkBehaviour : NetworkBehaviour
{
    public readonly SyncVar<string> LobbyId = new SyncVar<string>();
    public readonly SyncVar<string> LobbyName = new SyncVar<string>();
    public readonly SyncVar<int> MaxPlayers = new IntSyncVar() { Value = 4 };
    public readonly SyncVar<LobbyState> State = new(LobbyState.Open);

    public readonly SyncList<LobbyPlayer> Players = new SyncList<LobbyPlayer>();

    [Server]
    public void Initialize(string id, string lobbyName, int max, NetworkConnection ownerConn)
    {
        LobbyId.Value = id;
        LobbyName.Value = lobbyName;
        MaxPlayers.Value = max;
        State.Value = LobbyState.Open;


        Players.Add(new LobbyPlayer
            {
                connectionId = ownerConn.ClientId,
                displayName = $"P{ownerConn.ClientId}",
                ready = false
            }
        );
    }

    public void RequestJoinWrapper(NetworkConnection conn, out string reason)
    {
        reason = null;
        if (State.Value != LobbyState.Open)
        {
            reason = "Lobby closed";
            return;
        }

        if (Players.Count >= MaxPlayers.Value)
        {
            reason = "Full";
            return;
        }

        if (Players.Any(p => p.connectionId == conn.ClientId))
        {
            reason = "Already in lobby";
            return;
        }

        RequestJoin(conn);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestJoin(NetworkConnection conn)
    {
        Players.Add(new LobbyPlayer
        {
            connectionId = conn.ClientId,
            displayName = $"P{conn.ClientId}",
            ready = false
        });

        // if (State.Value != LobbyState.Open)
        // {
        //     TargetRpc_RejectJoin(conn, "Lobby closed");
        //     return;
        // }
        //
        // if (Players.Count >= MaxPlayers.Value)
        // {
        //     TargetRpc_RejectJoin(conn, "Full");
        //     return;
        // }
        //
        // if (Players.Any(p => p.connectionId == conn.ClientId))
        // {
        //     TargetRpc_RejectJoin(conn, "Already in lobby");
        //     return;
        // }
        //
        // Players.Add(new LobbyPlayer
        // {
        //     connectionId = conn.ClientId,
        //     displayName = $"P{conn.ClientId}",
        //     ready = false
        // });
        //
        // TargetRpc_AcceptJoin(conn, LobbyId.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReady(NetworkConnection conn, bool ready)
    {
        int idx = Players.FindIndex(p => p.connectionId == conn.ClientId);
        if (idx < 0) return;
        var p = Players[idx];
        p.ready = ready;
        Players[idx] = p;
    }

    public void RequestStartWrapper(NetworkConnection conn, out string reason)
    {
        reason = null;
        if (Players.Count == 0 || Players[0].connectionId != conn.ClientId)
        {
            reason = "Only host";
            return;
        }

        if (Players.Any(p => !p.ready))
        {
            reason = "Not all ready";
            return;
        }

        RequestStart(conn);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestStart(NetworkConnection conn)
    {
        State.Value = LobbyState.InGame;
    }

    [Server]
    public void RemovePlayerByClientId(int clientId)
    {
        int idx = Players.FindIndex(p => p.connectionId == clientId);
        if (idx >= 0) Players.RemoveAt(idx);
    }

    [TargetRpc]
    private void TargetRpc_RejectJoin(NetworkConnection target, string reason) =>
        UnityEngine.Debug.Log($"Join rejected: {reason}");

    [TargetRpc]
    private void TargetRpc_AcceptJoin(NetworkConnection target, string lobbyId) =>
        UnityEngine.Debug.Log($"Joined {lobbyId}");

    [TargetRpc]
    private void TargetRpc_Notify(NetworkConnection target, string msg) => UnityEngine.Debug.Log(msg);

    [TargetRpc]
    private void TargetRpc_StartGame(NetworkConnection target, string lobbyId) =>
        UnityEngine.Debug.Log($"Start game {lobbyId}");
}

[Serializable]
public struct LobbyPlayer : IEquatable<LobbyPlayer>
{
    public int connectionId;
    public string displayName;
    public bool ready;

    public bool Equals(LobbyPlayer other) =>
        connectionId == other.connectionId &&
        displayName == other.displayName &&
        ready == other.ready;
}

public enum LobbyState
{
    Open = 0,
    InGame = 1,
    Closed = 2
}