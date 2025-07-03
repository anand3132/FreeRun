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
    //From here cloud module handle the lobby
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
            SessionResponse response = await CloudModule.Instance.StartOrJoinTheLobby();
            string log = await CloudModule.Instance.GetAllocationServerLog();
            Debug.Log(log);
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

        bool IsSameSnapshot(List<PlayerData> a, List<PlayerData> b)
        {
            if (a == null || b == null || a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].PlayerId != b[i].PlayerId ||
                    a[i].JoinOrder != b[i].JoinOrder ||
                    a[i].IsLobbyReady != b[i].IsLobbyReady)
                {
                    return false;
                }
            }

            return true;
        }

        IEnumerator PollLobbyPlayers()
        {
            List<PlayerData> previousSnapshot = null;
            float timeout = 300f; // 5 minutes in seconds
            float elapsedTime = 0f;

            while (true)
            {
                Task<List<PlayerData>> fetchTask = CloudModule.Instance.FetchPlayersFromLobby(currentLobbyId);
                yield return new WaitUntil(() => fetchTask.IsCompleted);

                if (fetchTask.Exception != null)
                {
                    Debug.LogError($"Error fetching players: {fetchTask.Exception.Message}");
                }
                else
                {
                    List<PlayerData> currentSnapshot = fetchTask.Result;

                    if (currentSnapshot == null || currentSnapshot.Count == 0)
                    {
                        Debug.LogWarning("No players found in lobby.");
                    }
                    else
                    {
                        bool isLobbyReady = currentSnapshot[0].IsLobbyReady;

                        //if (!IsSameSnapshot(previousSnapshot, currentSnapshot))
                       // {
                            UpdatePlayerView(currentSnapshot);
                         //   previousSnapshot = new List<PlayerData>(currentSnapshot);
                      //  }

                        Debug.Log("🔄 Polling Lobby...");

                        if (isLobbyReady)
                        {
                            Debug.Log("✅ Lobby is full and server is ready. Stopping polling.");

                            Task<ServerAllocationResult> serverTask =
                                CloudModule.Instance.FetchAndHandleServerDetails(currentLobbyId);
                            yield return new WaitUntil(() => serverTask.IsCompleted);

                            if (serverTask.Exception != null)
                            {
                                Debug.LogError($"Error fetching server details: {serverTask.Exception.Message}");
                            }
                            else
                            {
                                ServerAllocationResult result = serverTask.Result;
                                Debug.Log($"🎯 Server ready: {result?.Ipv4}:{result?.GamePort}");
                                ConnectionManager.StartClient(result.Ipv4, (ushort)result.GamePort);

                                // Transition to game using server info
                            }

                            yield break;
                        }
                    }
                }

                // Timeout check
                elapsedTime += 3f; // Because we're waiting 3 seconds each loop
                if (elapsedTime >= timeout)
                {
                    Debug.LogWarning("⏰ Timeout reached. Exiting lobby polling after 5 minutes.");
                    // Optionally: leave the lobby or notify the player
                    CloudModule.Instance.LeaveLobby(currentLobbyId); // if implemented
                    yield break;
                }

                yield return new WaitForSeconds(3f);
            }
        }





    }
}

