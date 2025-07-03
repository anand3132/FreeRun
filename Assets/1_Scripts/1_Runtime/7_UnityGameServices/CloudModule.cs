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

        private void Awake()
        {
            _gameSessionModuleBinding = new GameSessionModuleBindings(CloudCodeService.Instance);
        }

        // public async Task<SessionResponse> StartTheGame()
        // {
        //     if (!AuthenticationService.Instance.IsSignedIn)
        //     {
        //         Debug.Log("User is not signed in");
        //         return null;
        //     }
        //
        //     try
        //     {
        //         var sessionResponse = await JoinMatchLobby();
        //         Debug.Log($"Lobby ID: {sessionResponse?.LobbyId} - {sessionResponse?.Message}");
        //         
        //         return sessionResponse;
        //     }
        //     catch (CloudCodeException exception)
        //     {
        //         Debug.LogError("CloudCode exception: " + exception.Message);
        //     }
        //
        //     return null;
        // }

        public async Task<SessionResponse> StartOrJoinTheLobby()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogError("User is not signed in.");
                return null;
            }

            string playerId = AuthenticationService.Instance.PlayerId;
            string playerName = AuthenticationService.Instance.PlayerName;
            string characterId = UserProfileManager.CurrentUser.CharacterId.ToString();

            try
            {
                var request = new SessionRequest()
                {
                    PlayerId = playerId,
                    CharacterId = characterId,
                    PlayerName = playerName
                };

                var result = await _gameSessionModuleBinding.StartOrJoinSession(request);

                if (result == null)
                {
                    Debug.LogError("Failed to join or create the lobby.");
                    return null;
                }

                Debug.Log($"Lobby Name: {result.LobbyName} | Lobby ID: {result.LobbyId} | Message: {result.Message}");

                return new SessionResponse
                {
                    LobbyId = result.LobbyId,
                    LobbyName = result.LobbyName,
                    Message = result.Message
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
                string allocationServerlog = await _gameSessionModuleBinding.GetDebugLog();
                return allocationServerlog;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to fetch Server Logs: {ex.Message}");
                return null;
            }

        }

        public async Task<List<PlayerData>> FetchPlayersFromLobby(string lobbyId)
        {
            if (!AuthenticationService.Instance.IsSignedIn || string.IsNullOrEmpty(lobbyId))
                return null;

            try
            {
                LobbyRequest request = new LobbyRequest()
                {
                    lobbyId = lobbyId
                };

                 List<PlayerSummary> summaries = await _gameSessionModuleBinding.GetLobbyPlayers(request);

                List<PlayerData> players = new List<PlayerData>(summaries.Count);
                foreach (var s in summaries)
                {
                    players.Add(new PlayerData
                    {
                        PlayerId = s.PlayerId,
                        DisplayName = s.DisplayName,
                        SelectedCharacterId = s.SelectedCharacterId,
                        IsLobbyReady = s.IsLobbyReady,
                        JoinOrder = s.JoinOrder,
                        MaxPlayersAllowed = s.MaxPlayersAllowed

                    });
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
    }
}
