using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RedGaint.Network.Runtime.ApplicationLifecycle;
using Unity.Netcode;
using Unity.Services.Multiplay;
using UnityEngine;

namespace RedGaint.Network.Runtime.ConnectionManagement
{
    [System.Serializable]
    public class AllocationPayload
    {
        public string LobbyId;
        public List<Unity.Services.Lobbies.Models.Player> Players;
    }

   
    class ServerListeningState : OnlineState
    {
        const int k_MaxConnectPayload = 1024;
        private AllocationPayload _allocationPayload;
        private bool _gameStarted = false;

        public override void Enter()
        {
            Debug.Log("<color=red>Server Listening State Entered</color>");
// #if UNITY_SERVER
            // Run async payload fetch logic in the background
            _ = Task.Run(async () =>
            {
                try
                {
                    // string allocationId = MultiplayService.Instance.ServerConfig.AllocationId;
                    string allocationId =MultiplayService.Instance.ServerConfig.AllocationId;
                    string payloadJson = await GetLocalPayloadAsync(allocationId);

                    if (!string.IsNullOrEmpty(payloadJson))
                    {
                        _allocationPayload = JsonUtility.FromJson<AllocationPayload>(payloadJson);
                        Debug.Log(
                            $"[Payload] LobbyId: {_allocationPayload.LobbyId}, Players: {_allocationPayload.Players?.Count ?? 0}");

                        if (_allocationPayload.Players != null)
                        {
                            foreach (var player in _allocationPayload.Players)
                            {
                                Debug.Log($"[Payload] Player ID: {player.Id}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[Payload] No payload received from Multiplay allocation.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Exception while fetching allocation payload: {ex.Message}");
                }
            });
// #endif
        }

        public async Task<string> GetLocalPayloadAsync(string allocationId)
        {
            using var client = new HttpClient();
            var url = $"http://localhost:8086/payload/{allocationId}";

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string payloadJson = await response.Content.ReadAsStringAsync();
                return payloadJson;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to fetch payload from local Multiplay API: {ex.Message}");
                return null;
            }
        }

        
        public override void Exit()
        {
            Debug.Log("<color=red>Exiting Server Listening State</color>");
        }

        public override void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[Server] Client {clientId} connected.");
            ConnectionManager.EventManager.Broadcast(new ClientConnectedEvent());

            int connectedCount = ConnectionManager.NetworkManager.ConnectedClientsIds.Count;
            int expectedCount = _allocationPayload?.Players?.Count ?? 0;

            Debug.Log($"[Server] Total connected clients: {connectedCount}/{expectedCount}");

            if (!_gameStarted && expectedCount > 0 && connectedCount >= expectedCount)
            {
                _gameStarted = true;
                Debug.Log("[Server] All players connected. Loading game scene...");
                ConnectionManager.NetworkManager.SceneManager.LoadScene("MainGameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            Debug.Log($"[Server] Client {clientId} disconnected.");
            ConnectionManager.EventManager.Broadcast(new ClientDisconnectedEvent());

            if (ConnectionManager.NetworkManager.ConnectedClientsIds.Count == 1 &&
                ConnectionManager.NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                Debug.Log("[Server] All clients disconnected. Shutting down.");
                ConnectionManager.EventManager.Broadcast(new ConnectionEvent { status = ConnectStatus.ServerEndedSession });
                ConnectionManager.ChangeState(ConnectionManager.m_Offline);
            }
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;

            if (connectionData.Length > k_MaxConnectPayload)
            {
                Debug.LogWarning("[Approval] Payload too large. Denying connection.");
                response.Approved = false;
                return;
            }

            var payload = Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            var status = GetConnectStatus(connectionPayload);

            if (status == ConnectStatus.Success)
            {
                response.Approved = true;
                response.CreatePlayerObject = true;
                response.Position = Vector3.zero;
                response.Rotation = Quaternion.identity;
            }
            else
            {
                response.Approved = false;
                response.Reason = JsonUtility.ToJson(status);
                Debug.LogWarning($"[Approval] Rejected client: {status}");
            }
        }

        private ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
        {
            if (ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= ApplicationEntryPoint.Singleton.MaxPlayers)
                return ConnectStatus.ServerFull;

            if (connectionPayload.applicationVersion != Application.version)
                return ConnectStatus.IncompatibleVersions;

            return ConnectStatus.Success;
        }

        public override void OnUserRequestedShutdown()
        {
            string reason = JsonUtility.ToJson(ConnectStatus.ServerEndedSession);

            foreach (var clientId in ConnectionManager.NetworkManager.ConnectedClientsIds)
            {
                ConnectionManager.NetworkManager.DisconnectClient(clientId, reason);
            }

            ConnectionManager.EventManager.Broadcast(new ConnectionEvent { status = ConnectStatus.ServerEndedSession });
            ConnectionManager.ChangeState(ConnectionManager.m_Offline);
        }

        public override void OnServerStopped()
        {
            Debug.Log("[Server] Stopped.");
            ConnectionManager.EventManager.Broadcast(new ConnectionEvent { status = ConnectStatus.GenericDisconnect });
            ConnectionManager.ChangeState(ConnectionManager.m_Offline);
        }
    }
}
