using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
        public static LobbyManager Instance;
        public NetworkObject lobbyPrefab;

        private readonly Dictionary<string, LobbyNetworkBehaviour> lobbies = new();


        void Awake() => Instance = this;

        public override void OnStartServer()
        {
            base.OnStartServer();
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        }

        public override void OnStopServer()
        {
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            base.OnStopServer();
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState != RemoteConnectionState.Stopped) return;

            int clientId = connection.ClientId;
            foreach (var kv in lobbies.ToArray())
            {
                var lobby = kv.Value;
                if (lobby == null) continue;

                lobby.RemovePlayerByClientId(clientId);

                // destroy lobby if empty
                if (lobby.Players.Count == 0)
                {
                    DestroyLobby(kv.Key);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreateLobby(string lobbyName, int maxPlayers, NetworkConnection rConn = null)
        {
            if (rConn == null) return;

            string id = Guid.NewGuid().ToString();
            var networkObject = Instantiate(lobbyPrefab);
            var lobby = networkObject.GetComponent<LobbyNetworkBehaviour>();

            NetworkObject.Spawn(networkObject);
            lobby.Initialize(id, lobbyName, maxPlayers, rConn);
            lobbies.Add(id, lobby);

            var clientCtrl = rConn.FirstObject?.GetComponent<PlayerNetwork>();
            clientCtrl?.ReceiveCreatedLobby(rConn, id, NetworkObject.ObjectId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLobbyList(NetworkConnection rConn = null)
        {
            if (rConn == null) return;

            var lines = lobbies.Values.Select(l
                    => $"{l.LobbyId.Value}|" +
                       $"{l.LobbyName.Value}|" +
                       $"{l.Players.Count}|" +
                       $"{l.MaxPlayers.Value}|" +
                       $"{(int)l.State.Value}|" +
                       $"{l.NetworkObject.ObjectId}" //TODO check dodginess
            );

            var payload = string.Join(";", lines);
            var clientCtrl = rConn.FirstObject?.GetComponent<PlayerNetwork>();
            clientCtrl?.ReceiveLobbyList(rConn, payload);
        }
    
        
        [ServerRpc(RequireOwnership = false)]
        public void RequestJoinByNetworkId(uint lobbyNetworkObjectId, NetworkConnection rConn = null)
        {
            if (rConn == null) return;

            var lobby = GetLobbyByNetworkId(lobbyNetworkObjectId);
            var clientPlayer = rConn.FirstObject;
            var playerComp = clientPlayer?.GetComponent<PlayerNetwork>();

            if (lobby == null)
            {
                playerComp?.Notify(rConn, "Lobby not found");
                return;
            }

            lobby.RequestJoinWrapper(rConn, out var reason);
            if (reason != null)
            {
                playerComp?.Notify(rConn, reason);
                return;
            }

            playerComp?.AcceptJoin(rConn, lobby.LobbyId.Value, lobby.NetworkObject.ObjectId);
        }
    
        [ServerRpc(RequireOwnership = false)]
        public void SetReady(uint lobbyNetworkObjectId, bool ready, NetworkConnection rConn = null)
        {
            if (rConn == null) return;
            var lobby = GetLobbyByNetworkId(lobbyNetworkObjectId);
            if (lobby == null) return;
            lobby.SetReady(rConn, ready);
        }
    
        [ServerRpc(RequireOwnership = false)]
        public void RequestStart(uint lobbyNetworkObjectId, NetworkConnection rConn = null)
        {
            if (rConn == null) return;
            var lobby = GetLobbyByNetworkId(lobbyNetworkObjectId);
            var clientPlayer = rConn.FirstObject;
            var playerComp = clientPlayer?.GetComponent<PlayerNetwork>();
            if (lobby == null)
            {
                playerComp?.Notify(rConn, "Lobby not found"); 
                return;
            }

            lobby.RequestStartWrapper(rConn, out string reason);
            if (reason != null)
            {
                playerComp?.Notify(rConn, reason);
                return;
            }

            foreach (var p in lobby.Players)
            {
                var conn = ServerManager.Clients[p.connectionId];
                if (conn == null) continue;

                // Use your player prefab from a Resources folder or a field reference
                var playerPrefab = Resources.Load<GameObject>("Player");
                var player = Instantiate(playerPrefab, GetSpawnPoint(p.connectionId), Quaternion.identity);
                ServerManager.Spawn(player, conn);
            }
            
            foreach (var p in lobby.Players)
            {
                var conn = ServerManager.Clients[p.connectionId];
                var target = conn?.FirstObject?.GetComponent<PlayerNetwork>();
                target?.StartGame(conn, lobby.LobbyId.Value);
            }
        }
    
        private Vector3 GetSpawnPoint(int connectionId)
        {
            // simple spawn logic
            var spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
            if (spawns.Length == 0)
                return Vector3.zero;
            return spawns[connectionId % spawns.Length].transform.position;
        }
        
        [Server]
        private void DestroyLobby(string id)
        {
            if (!lobbies.Remove(id, out var lobby)) return;
            if (lobby != null && lobby.NetworkObject != null && lobby.NetworkObject.IsSpawned)
                lobby.NetworkObject.Despawn();
        }

        public LobbyNetworkBehaviour GetLobbyByNetworkId(uint networkId)
        {
            return lobbies.Values.FirstOrDefault(l =>
                l != null && l.NetworkObject != null && l.NetworkObject.ObjectId == networkId);
        }
    
        public LobbyNetworkBehaviour GetLobbyById(string id)
        {
            lobbies.TryGetValue(id, out var l);
            return l;
        }
}
