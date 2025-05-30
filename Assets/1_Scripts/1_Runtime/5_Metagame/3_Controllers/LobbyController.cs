using RedGaint.Network.Runtime.ApplicationLifecycle;
using RedGaint.Network.Runtime.ConnectionManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedGaint.Network.Runtime.UserData;
using Unity.Services.CloudCode;
using Unity.Services.Authentication;
using System.Collections;

namespace RedGaint.Network.Runtime
{
    internal class LobbyController : Controller<MetagameApplication>
    {
        LobbyView View => App.View.LobbyView;
        ConnectionManager ConnectionManager => ApplicationEntryPoint.Singleton.ConnectionManager;

        private float countdownTime;
        private Coroutine lobbyPollCoroutine;
        private string currentLobbyId;

        void Awake()
        {
            AddListener<EnterLobbyQueueEvent>(OnLobbyEntered);
            AddListener<LobbyStartedEvent>(OnLobbyStarted);
            AddListener<LobbyCountdownUpdateEvent>(OnLobbyCountdownUpdate);
            AddListener<LobbyGameStartingEvent>(OnLobbyGameStarting);
            ConnectionManager.EventManager.AddListener<ConnectionEvent>(OnConnectionEvent);
        }

        private void OnLobbyEntered(EnterLobbyQueueEvent obj)
        {
            Stage.Instance.ClearAllTables();
            View.Show();
            Broadcast(new LobbyStartedEvent(30)); // Trigger countdown start
        }

        void OnDestroy()
        {
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

        void OnLobbyStarted(LobbyStartedEvent evt)
        {
            countdownTime = evt.CountdownTime;
            View.UpdateCountdown(countdownTime);

            _ = StartLobbySession();
        }

        async Task StartLobbySession()
        {
            Debug.Log($"Starting lobby session for :  {AuthenticationService.Instance.PlayerId}");
            SessionResponse response = await CloudModule.Instance.StartTheGame();
            currentLobbyId = response?.LobbyId;

            if (!string.IsNullOrEmpty(currentLobbyId))
            {
                if (lobbyPollCoroutine != null)
                    StopCoroutine(lobbyPollCoroutine);

                lobbyPollCoroutine = StartCoroutine(PollLobbyPlayers());
            }
        }

        void OnLobbyCountdownUpdate(LobbyCountdownUpdateEvent evt)
        {
            countdownTime = evt.SecondsRemaining;
            View.UpdateCountdown(countdownTime);
        }

        void OnLobbyGameStarting(LobbyGameStartingEvent evt)
        {
            View.Hide();
            StopLobby(); // Stop polling

            SceneManager.UnloadSceneAsync(GlobalStaticVariables.MetaScene);
            SceneManager.LoadSceneAsync(GlobalStaticVariables.GameScene);
        }

        void OnConnectionEvent(ConnectionEvent evt)
        {
            if (evt.status == ConnectStatus.Connecting)
            {
                View.Hide();
                StopLobby();
            }
        }

        void StopLobby()
        {
            if (lobbyPollCoroutine != null)
            {
                StopCoroutine(lobbyPollCoroutine);
                lobbyPollCoroutine = null;
            }
        }
        
        void UpdatePlayerView(List<PlayerData> players)
        {
            if (players != null)
            {
                View.UpdatePlayerList(players);
            }
            else
            {
                Debug.LogWarning("No players to update in the view.");
            }
        }

        IEnumerator PollLobbyPlayers()
        {
            while (true)
            {
                var fetchTask = CloudModule.Instance.FetchPlayersFromLobby(currentLobbyId);
                yield return new WaitUntil(() => fetchTask.IsCompleted);
                // yield return new WaitForSeconds(1f);
                if (fetchTask.Exception != null)
                {
                    Debug.Log($"Error fetching players: {fetchTask.Exception.Message}");
                }
                else
                {
                    UpdatePlayerView(fetchTask.Result);
                }

                yield return new WaitForSeconds(3f);
            }
        }

        
    }
}

