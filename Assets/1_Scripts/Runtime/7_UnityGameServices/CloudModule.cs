using System.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;
using System.Collections.Generic;
using RedGaint.Utility;
using Unity.Services.Authentication;
using Unity.Services.CloudCode.GeneratedBindings.RedGaint.Network.GameSessionModule;

namespace RedGaint.Network.Runtime.UserData
{
    public class CloudModule : Singleton<CloudModule>, IBugsBunny
    {
        public async Task<SessionResponse> StartTheGame()
        {
            await TestCloudCode();
            
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("User is not signed in");
                return null;
            }
         
            
            try
            {
                // Here we initialize the Cloud Code service
                var cloudCodeService = CloudCodeService.Instance;

                // Call Cloud Code function (you can call the function here or elsewhere based on your design)
                var sessionResponse = await JoinMatchLobby();
                Debug.Log($"Lobby ID: {sessionResponse.LobbyId} - {sessionResponse.Message}");
                return sessionResponse;
            }
            catch (CloudCodeException exception)
            {
                Debug.LogError("CloudCode exception: " + exception.Message);
            }
            return null;
        }

        public async Task<SessionResponse> JoinMatchLobby()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogError("User is not signed in.");
                return null;
            }

            // Get the required information
            string accessToken = AuthenticationService.Instance.AccessToken;
            string playerId = AuthenticationService.Instance.PlayerId;  // Or fetch it differently if needed
    
            // Dummy values for playerName and characterId
            string playerName = "DummyPlayer_"+Random.Range(1000, 9999);  // Dummy player name
            string characterId = "DummyCharacter";  // Dummy character ID

            // Prepare the arguments to send to the Cloud Code
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "AccessToken", accessToken },
                { "PlayerId", playerId },
                { "CharacterId", characterId },
                { "PlayerName", playerName }
            };

            try
            {
                
                // Prepare the request object for the StartOrJoinSession function
                var request = new GameSession_SessionRequest()
                {
                    PlayerId = playerId,
                    CharacterId = characterId,
                    PlayerName = playerName
                };
                
                var gameSessionBindings = new GameSessionModuleBindings(CloudCodeService.Instance);

                // Call the StartOrJoinSession function
                var result = await gameSessionBindings.StartOrJoinSession(request);
                
                // Check if the result is not null
                if (result != null)
                {
                    
                    
                    Debug.Log($"Lobby Name : {result.LobbyName} : Lobby ID: {result.LobbyId} :: Message: {result.Message}");
                }
                else
                {
                    Debug.LogError("Failed to join or create the lobby.");
                }

                SessionResponse response = new SessionResponse
                {
                    LobbyId = result.LobbyId,
                    LobbyName = result.LobbyName,
                    Message = result.Message,
                };
                
                return response;
            }
            catch (CloudCodeException ex)
            {
                Debug.LogError($"Cloud Code exception: {ex.Message}");
                return null;
            }
        }

        private async Task<string> TestCloudCode()
        {
            Debug.Log("Queriing.....");
            // Call the function within the module and provide the parameters we defined in there
            var module = new GameSessionModuleBindings(CloudCodeService.Instance);
            var result1 = await module.SayHello("World");
            Debug.Log(result1);
            return result1;
        }


        public bool LogThisClass { get; } = false;
    }
}

public class SessionResponse
{
    public string LobbyId { get; set; }
    public string LobbyName { get; set; }
    public string Message { get; set; }
}
