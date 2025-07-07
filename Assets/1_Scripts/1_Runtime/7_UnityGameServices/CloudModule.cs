using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudCode.GeneratedBindings.RedGaint.Network.GameSessionModule;
using Unity.Services.Authentication;
using RedGaint.Utility;

namespace RedGaint.Network.Runtime.UserData
{
    public class CloudModule : Singleton<CloudModule>, IBugsBunny
    {
        private GameSessionModuleBindings _gameSessionModuleBinding;

        public bool LogThisClass=> false;

        public override void Awake()
        {
            base.Awake();
            _gameSessionModuleBinding = new GameSessionModuleBindings(CloudCodeService.Instance);
        }

        public async Task<SessionResponse> StartOrJoinTheLobby()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogError("User is not signed in.");
                return null;
            }

            string playerId = AuthenticationService.Instance.PlayerId;
            string playerName = UserProfileManager.CurrentUser.Username;
            string characterId = UserProfileManager.CurrentUser.CharacterId.ToString();

            try
            {
                
                var request = new SessionRequest()
                {
                    PlayerId = playerId,
                    CharacterId = characterId,
                    PlayerName = playerName,
                    Xp=100
                };

                var result = await _gameSessionModuleBinding.StartOrJoinSession(request);
                Debug.Log("-----------------------------------");

                Debug.Log($"result: {result.Message}");
                
                if (result == null)
                {
                    Debug.LogError("Failed to join or create the lobby.");
                    return null;
                }

                Debug.Log($"Lobby Name: {result.LobbyName} | Lobby ID: {result.LobbyId} | Message: {result.Message}");

                List<PlayerSummary> Players = new List<PlayerSummary>();
                foreach (var player in result.Players)
                {
                    
                    Players.Add(new PlayerSummary()
                    {
                        PlayerId = player.PlayerId,
                        DisplayName = player.DisplayName,
                        IsLobbyReady = player.IsLobbyReady,
                        MaxPlayersAllowed = player.MaxPlayersAllowed,
                        SelectedCharacterId = player.SelectedCharacterId,
                        JoinOrder = player.JoinOrder,
                    });
                }
                
                return new SessionResponse
                {
                    LobbyId = result.LobbyId,
                    LobbyName = result.LobbyName,
                    Message = result.Message,
                    Players = Players
                };
            }
            catch (CloudCodeException ex)
            {
                Debug.LogError($"Cloud Code exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAllocationServerLog()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
                return null;
            try
            {
                string allocationServerlog = await _gameSessionModuleBinding.GetAllocationServerLogs();
                return allocationServerlog;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to fetch Server Logs: {ex.Message}");
                return null;
            }

        }

        public async Task<string> ClearAllocationServerLog()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
                return null;
            try
            {
                string allocationServerlog = await _gameSessionModuleBinding.ClearAllocationServerLogs();
                return allocationServerlog;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to fetch Server Logs: {ex.Message}");
                return null;
            }

        }
        public async Task<List<PlayerSummary>> FetchPlayersFromLobby(string lobbyId)
        {
            if (!AuthenticationService.Instance.IsSignedIn || string.IsNullOrEmpty(lobbyId))
                return null;

            try
            {
                LobbyRequest request = new LobbyRequest()
                {
                    lobbyId = lobbyId
                };

                 List<Unity.Services.CloudCode.GeneratedBindings.RedGaint.Network.GameSessionModule.PlayerSummary> summaries = await _gameSessionModuleBinding.GetLobbyPlayers(request);

                List<PlayerSummary> players = new List<PlayerSummary>(summaries.Count);
                foreach (var s in summaries)
                {
                    players.Add(new PlayerSummary
                    {
                        PlayerId = s.PlayerId,
                        DisplayName = s.DisplayName,
                        SelectedCharacterId = s.SelectedCharacterId,
                        IsLobbyReady = s.IsLobbyReady,
                        JoinOrder = s.JoinOrder,
                        MaxPlayersAllowed = s.MaxPlayersAllowed

                    });
                    Debug.Log($"<color=red>Player Name : {s.DisplayName} </color>");
                }
                return players;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to fetch player list: {ex.Message}");
                return null;
            }
        }

        public async Task<ServerAllocationResult> FetchAndHandleServerDetails(string lobbyId)
        {
            if (!AuthenticationService.Instance.IsSignedIn || string.IsNullOrEmpty(lobbyId))
                return null;

            try
            {
                LobbyRequest request = new LobbyRequest
                {
                    lobbyId = lobbyId
                };

                var allocationResult = await _gameSessionModuleBinding.GetServerDetails(request);

                ServerAllocationResult allocationDetails = new ServerAllocationResult
                {
                    AllocationId = allocationResult.AllocationId,
                    ServerId = allocationResult.ServerId,
                    Ipv4 = allocationResult.Ipv4,
                    Ipv6 = allocationResult.Ipv6,
                    GamePort = allocationResult.GamePort,
                    LobbyId = allocationResult.LobbyId
                };

                return allocationDetails;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to fetch server details for lobby {lobbyId}: {ex.Message}");
                return null;
            }
        }

        public async Task<string>  LeaveLobby(string lobbyId)
        {
            if (!AuthenticationService.Instance.IsSignedIn || string.IsNullOrEmpty(lobbyId))
                return null;
            try
            {
                LobbyRequest request = new LobbyRequest
                {
                    lobbyId = lobbyId
                };
                
                return await _gameSessionModuleBinding.EndGameSession(request);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Internal Error while exiting  lobby {lobbyId}: {ex.Message}");
                throw;
            }

        }

        public async Task<string> TestAllocation()
        {
            var lobyid=await _gameSessionModuleBinding.CreateTestLobbyWithBots(2);
            Debug.Log($"<color=red>Test Allocation Success!:{lobyid}</color>");
            return await _gameSessionModuleBinding.GetAllocationServerLogs();
        }

    }
}
