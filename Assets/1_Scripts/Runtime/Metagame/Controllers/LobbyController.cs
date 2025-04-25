// using System;
// using RedGaint.Network.Runtime.ApplicationLifecycle;
// using RedGaint.Network.Runtime.ConnectionManagement;
// using UnityEngine;
// using UnityEngine.SceneManagement;
//
// namespace RedGaint.Network.Runtime
// {
//     internal class LobbyController : Controller<MetagameApplication>
//     {
//         // View for Lobby (Accessible through App.View.Lobby)
//         LobbyView View => App.View.LobbyView;
//         
//         ConnectionManager ConnectionManager => ApplicationEntryPoint.Singleton.ConnectionManager;
//
//         // Countdown time (For managing the timer for the lobby)
//         private float countdownTime;
//
//         void Awake()
//         {
//             // Add event listeners for the lobby
//             AddListener<EnterLobbyQueueEvent>(OnLobbyEntered);
//             AddListener<LobbyStartedEvent>(OnLobbyStarted);
//             
//             AddListener<LobbyCountdownUpdateEvent>(OnLobbyCountdownUpdate);
//             AddListener<LobbyGameStartingEvent>(OnLobbyGameStarting);
//             ConnectionManager.EventManager.AddListener<ConnectionEvent>(OnConnectionEvent);
//         }
//
//         private void OnLobbyEntered(EnterLobbyQueueEvent obj)
//         {
//             View.ShowLobby();
//             
//         }
//
//         void OnDestroy()
//         {
//             // Remove event listeners when the controller is destroyed
//             RemoveListeners();
//         }
//
//         void OnApplicationQuit()
//         {
//             StopLobby();
//         }
//
//         internal override void RemoveListeners()
//         {
//             RemoveListener<EnterLobbyQueueEvent>(OnLobbyEntered);
//             RemoveListener<LobbyStartedEvent>(OnLobbyStarted);
//             RemoveListener<LobbyCountdownUpdateEvent>(OnLobbyCountdownUpdate);
//             RemoveListener<LobbyGameStartingEvent>(OnLobbyGameStarting);
//             ConnectionManager.EventManager.RemoveListener<ConnectionEvent>(OnConnectionEvent);
//         }
//
//         // Event handler when the lobby is started
//         void OnLobbyStarted(LobbyStartedEvent evt)
//         {
//             countdownTime = evt.CountdownTime;
//
//             // // Show the lobby view
//             // View.ShowLobby();
//
//             // Start the countdown in the view
//             View.UpdateCountdown(evt.CountdownTime);
//         }
//
//         // Event handler when the countdown is updated
//         void OnLobbyCountdownUpdate(LobbyCountdownUpdateEvent evt)
//         {
//             countdownTime = evt.SecondsRemaining;
//
//             // Update the countdown in the lobby view
//             View.UpdateCountdown(evt.SecondsRemaining);
//         }
//
//         // Event handler when the game is about to start
//         void OnLobbyGameStarting(LobbyGameStartingEvent evt)
//         {
//             // Hide the lobby view
//             View.HideLobby();
//
//             // Unload the lobby scene and load the game scene
//             SceneManager.UnloadSceneAsync("LobbyScene");
//             SceneManager.LoadSceneAsync("GameScene");
//         }
//
//         // Handle connection events (e.g., disconnect, connecting)
//         void OnConnectionEvent(ConnectionEvent evt)
//         {
//             if (evt.status == ConnectStatus.Connecting)
//             {
//                 View.HideLobby();
//             }
//         }
//
//         // Stop the lobby (e.g., during disconnection or when leaving the lobby)
//         void StopLobby()
//         {
//             // Logic to stop the lobby, possibly by cleaning up resources or stopping any background tasks
//         }
//     }
// }
