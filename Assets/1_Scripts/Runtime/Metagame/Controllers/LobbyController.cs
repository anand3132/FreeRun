using RedGaint.Network.Runtime.ApplicationLifecycle;
using RedGaint.Network.Runtime.ConnectionManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedGaint.Network.Runtime.UserData;
using Unity.Services.CloudCode;
using Unity.Services.Authentication;
namespace RedGaint.Network.Runtime
{
    internal class LobbyController : Controller<MetagameApplication>
    {
        // View for Lobby (Accessible through App.View.Lobby)
        LobbyView View => App.View.LobbyView;
        public GameObject LobbyStage;

        ConnectionManager ConnectionManager => ApplicationEntryPoint.Singleton.ConnectionManager;

        // Countdown time (For managing the timer for the lobby)
        private float countdownTime;

        void Awake()
        {
            // Add event listeners for the lobby
            AddListener<EnterLobbyQueueEvent>(OnLobbyEntered);
            
            AddListener<LobbyStartedEvent>(OnLobbyStarted);
            AddListener<LobbyCountdownUpdateEvent>(OnLobbyCountdownUpdate);
            AddListener<LobbyGameStartingEvent>(OnLobbyGameStarting);
            ConnectionManager.EventManager.AddListener<ConnectionEvent>(OnConnectionEvent);
        }

        private void OnLobbyEntered(EnterLobbyQueueEvent obj)
        {
            //set up the user pfofile and initialte lobby creation and wait fro the cloud code to get game start
            View.Show();
            LobbyStage.SetActive(true);
            Broadcast(new LobbyStartedEvent(30));
            
        }

        void OnDestroy()
        {
            // Remove event listeners when the controller is destroyed
            RemoveListeners();
        }

        void OnApplicationQuit()
        {
            StopLobby();
        }

        internal override void RemoveListeners()
        {
            RemoveListener<EnterLobbyQueueEvent>(OnLobbyEntered);
            RemoveListener<LobbyStartedEvent>(OnLobbyStarted);
            RemoveListener<LobbyCountdownUpdateEvent>(OnLobbyCountdownUpdate);
            RemoveListener<LobbyGameStartingEvent>(OnLobbyGameStarting);
            
            ConnectionManager.EventManager.RemoveListener<ConnectionEvent>(OnConnectionEvent);
        }

        // Event handler when the lobby is started
        void OnLobbyStarted(LobbyStartedEvent evt)
        {
            countdownTime = evt.CountdownTime;
            // Start the countdown in the view
            View.UpdateCountdown(evt.CountdownTime);

            Task SessionResponse = CloudModule.Instance.StartTheGame();
            //JoinOrCreateLobby();

        }

        // Event handler when the countdown is updated
        void OnLobbyCountdownUpdate(LobbyCountdownUpdateEvent evt)
        {
            countdownTime = evt.SecondsRemaining;
            
            // Update the countdown in the lobby view
            View.UpdateCountdown(evt.SecondsRemaining);
        }

        // Event handler when the game is about to start
        void OnLobbyGameStarting(LobbyGameStartingEvent evt)
        {
            // Hide the lobby view
            View.Hide();
            // Unload the lobby scene and load the game scene
            SceneManager.UnloadSceneAsync(GlobalStaticVariables.MetaScene);
            SceneManager.LoadSceneAsync(GlobalStaticVariables.GameScene);
        }

        // Handle connection events (e.g., disconnect, connecting)
        void OnConnectionEvent(ConnectionEvent evt)
        {
            if (evt.status == ConnectStatus.Connecting)
            {
                View.Hide();
            }
        }

        // Stop the lobby (e.g., during disconnection or when leaving the lobby)
        void StopLobby()
        {
            // Logic to stop the lobby, possibly by cleaning up resources or stopping any background tasks
        }
        
        
        async Task JoinOrCreateLobby()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
                return;

            var playerId = AuthenticationService.Instance.PlayerId;
            var username = "Player_" + Random.Range(0, 9999); // 🚀 (You can use stored username if you have)
            var characterId = "Warrior"; // 🚀 (Replace with selected character if available)

            var result = await CloudCodeService.Instance.CallEndpointAsync<Dictionary<string, string>>(
                "StartOrJoinLobby",
                new Dictionary<string, object>
                {
                    { "playerId", playerId },
                    { "playerName", username },
                    { "characterId", characterId }
                }
            );

            string lobbyId = result["lobbyId"];
            Debug.Log($"Successfully joined or created lobby: {lobbyId}");
        }
    }
}
