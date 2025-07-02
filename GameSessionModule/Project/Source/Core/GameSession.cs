using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;

namespace RedGaint.Network.GameSessionModule
{
    /// <summary>
    /// Entry point for Unity Cloud Code multiplayer session logic.
    /// This class wires together Cloud Code functions and delegates execution
    /// to the appropriate services (e.g., LobbyService).
    /// </summary>
    public class GameSession
    {
        private readonly LobbyService _lobbyService;
        private readonly ILogger<GameSession> _logger;

        /// <summary>
        /// Constructor for GameSession.
        /// Dependencies are injected via Unity Cloud Code's DI system.
        /// </summary>
        /// <param name="lobbyService">Service that handles lobby creation/joining.</param>
        /// <param name="logger">Logger for tracking session events and errors.</param>
        public GameSession(LobbyService lobbyService, ILogger<GameSession> logger)
        {
            _lobbyService = lobbyService;
            _logger = logger;
        }

        /// <summary>
        /// Cloud Code entry point to start or join a game session.
        /// - Finds an existing matching lobby or creates a new one.
        /// - Triggers lobby creation, joining logic, and fallback handling.
        /// </summary>
        /// <param name="ctx">Execution context containing player and auth info.</param>
        /// <param name="request">Data sent from the client (PlayerId, CharacterId, XP, etc.).</param>
        /// <returns>SessionResponse containing lobby details or error info.</returns>
        [CloudCodeFunction("StartOrJoinSession")]
        public async Task<SessionResponse> StartOrJoinSession(IExecutionContext ctx, SessionRequest request)
        {
            try
            {
                CloudDebugLogger.Log("StartOrJoinSession started ...");
                return await _lobbyService.HandleStartOrJoinSession(ctx, request);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to start or join session");
                return new SessionResponse { Message = $"Error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Cloud Code function to fetch all players currently in a specified lobby.
        /// Useful for in-lobby UI or team selection before starting the match.
        /// </summary>
        /// <param name="ctx">Cloud Code execution context.</param>
        /// <param name="request">Contains the lobby ID to query.</param>
        /// <returns>List of players with ID, name, and character info.</returns>
        [CloudCodeFunction("GetLobbyPlayers")]
        public async Task<List<PlayerSummary>> GetLobbyPlayers(IExecutionContext ctx, LobbyRequest request)
        {
            return await _lobbyService.GetLobbyPlayers(ctx, request.lobbyId);
        }


        [CloudCodeFunction("GetDebugLog")]
        public  async Task<string> GetDebugLog(IExecutionContext ctx)
        {
            return await Task.FromResult(CloudDebugLogger.GetRecentLogs());
        }

        [CloudCodeFunction("ClearDebugLog")]
        public  async Task<string> ClearDebugLog(IExecutionContext ctx)
        {
            CloudDebugLogger.Clear();
            return await Task.FromResult("Log file cleared.");
        }
        
        
     
        
        
        
    }
}


   //
   // [CloudCodeFunction("GetAllocatedServer")]
   //      public ServerStatusResponse GetAllocatedServer(LobbyQueryRequest request)
   //      {
   //          try
   //          {
   //              if (string.IsNullOrEmpty(request?.LobbyId))
   //              {
   //                  _logger.LogError("Invalid lobby ID received");
   //                  return new ServerStatusResponse
   //                  {
   //                      StatusMessage = "Invalid lobby ID"
   //                  };
   //              }
   //
   //              var allocation = _serverService.GetAllocationForLobby(request.LobbyId);
   //              
   //              if (allocation == null)
   //              {
   //                  return new ServerStatusResponse
   //                  {
   //                      LobbyId = request.LobbyId,
   //                      AllocationStatus = "pending",
   //                      StatusMessage = "Server allocation in progress"
   //                  };
   //              }
   //
   //              return new ServerStatusResponse
   //              {
   //                  LobbyId = request.LobbyId,
   //                  AllocationStatus = "ready",
   //                  ServerIP = allocation.IPv4Address,
   //                  Port = allocation.GamePort,
   //                  ConnectionToken = allocation.AllocationId,
   //                  StatusMessage = "Server ready for connection"
   //              };
   //          }
   //          catch (System.Exception ex)
   //          {
   //              _logger.LogError(ex, $"Failed to get server allocation for lobby {request?.LobbyId}");
   //              return new ServerStatusResponse
   //              {
   //                  LobbyId = request?.LobbyId,
   //                  AllocationStatus = "failed",
   //                  StatusMessage = $"Error: {ex.Message}"
   //              };
   //          }
   //      }
   //
   //      [CloudCodeFunction("CleanupServerAllocation")]
   //      public ServerStatusResponse CleanupServerAllocation(LobbyQueryRequest request)
   //      {
   //          try
   //          {
   //              if (string.IsNullOrEmpty(request?.LobbyId))
   //              {
   //                  _logger.LogError("Invalid lobby ID received");
   //                  return new ServerStatusResponse
   //                  {
   //                      StatusMessage = "Invalid lobby ID"
   //                  };
   //              }
   //
   //              _serverService.CleanupLobbyAllocation(request.LobbyId);
   //              
   //              return new ServerStatusResponse
   //              {
   //                  LobbyId = request.LobbyId,
   //                  AllocationStatus = "cleaned",
   //                  StatusMessage = "Server allocation cleaned up"
   //              };
   //          }
   //          catch (System.Exception ex)
   //          {
   //              _logger.LogError(ex, $"Failed to cleanup allocation for lobby {request?.LobbyId}");
   //              return new ServerStatusResponse
   //              {
   //                  LobbyId = request?.LobbyId,
   //                  AllocationStatus = "error",
   //                  StatusMessage = $"Cleanup failed: {ex.Message}"
   //              };
   //          }
   //      }