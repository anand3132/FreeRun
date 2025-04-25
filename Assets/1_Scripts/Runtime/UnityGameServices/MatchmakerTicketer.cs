using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using StatusOptions = Unity.Services.Matchmaker.Models.MultiplayAssignment.StatusOptions;

namespace RedGaint.Network.Runtime
{
    internal class MatchmakerTicketer : MonoBehaviour
    {
        internal string LastQueueName { get; private set; }
        internal bool Searching { get; private set; }

        private string m_TicketId = "";
       
        private int m_LocalCharacterId = -1;

        /// <summary>
        /// Starts matchmaking with the selected characterId.
        /// </summary>
        public async void FindMatch(
            string queueName,
            int characterId,
            Action<MultiplayAssignment, List<(string playerId, int characterId)>> onMatchSearchCompleted,
            Action<int> onMatchmakerTicked)
        {
            if (Searching || !AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("Already searching or not signed in.");
                return;
            }

            Searching = true;
            m_LocalCharacterId = characterId;

            try
            {
                await StartSearch(queueName, characterId, onMatchSearchCompleted, onMatchmakerTicked);
            }
            catch (Exception e)
            {
                Debug.LogError($"Matchmaker error: {e.Message}");
                await StopSearch();
                MetagameApplication.Instance.Broadcast(new ExitMatchmakerQueueEvent());
            }
        }

        private async Task StartSearch(
            string queueName,
            int characterId,
            Action<MultiplayAssignment, List<(string playerId, int characterId)>> onMatchSearchCompleted,
            Action<int> onMatchmakerTicked)
        {
            LastQueueName = queueName;

            var attributes = new Dictionary<string, object>();
            var playerAttributes = new Dictionary<string, object> { { "characterId", characterId } };

            var players = new List<Player>
            {
                new Player(AuthenticationService.Instance.PlayerId, playerAttributes)
            };

            var options = new CreateTicketOptions(queueName, attributes);
            var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);

            m_TicketId = ticketResponse.Id;
            Debug.Log($"Matchmaker Ticket Created: {m_TicketId}");

            _ = PollTicketStatusAsync(onMatchSearchCompleted, onMatchmakerTicked);
        }

        public async Task StopSearch()
        {
            if (!string.IsNullOrEmpty(m_TicketId))
            {
                try
                {
                    await MatchmakerService.Instance.DeleteTicketAsync(m_TicketId);
                    Debug.Log($"Matchmaker ticket {m_TicketId} deleted.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to delete matchmaking ticket: {e.Message}");
                }

                m_TicketId = string.Empty;
            }

            Searching = false;
        }


        private async Task PollTicketStatusAsync(
            Action<MultiplayAssignment, List<(string playerId, int characterId)>> onMatchSearchCompleted,
            Action<int> onMatchmakerTicked)
        {
            int elapsed = 0;
            bool polling = true;

            while (polling)
            {
                await Task.Delay(3000);
                elapsed++;
                onMatchmakerTicked?.Invoke(elapsed);

                try
                {
                    var statusResponse = await MatchmakerService.Instance.GetTicketAsync(m_TicketId);
                    var assignment = statusResponse.Value as MultiplayAssignment;

                    if (assignment == null) continue;

                    switch (assignment.Status)
                    {
                        case StatusOptions.InProgress:
                            Debug.Log("Matchmaker status: InProgress");
                            break;

                        case StatusOptions.Found:
                            Debug.Log("Matchmaker status: Found");
                            polling = false;

                            var matchedPlayers = new List<(string, int)>
                            {
                                (AuthenticationService.Instance.PlayerId, m_LocalCharacterId)
                            };

                            onMatchSearchCompleted?.Invoke(assignment, matchedPlayers);
                            break;

                        case StatusOptions.Failed:
                        case StatusOptions.Timeout:
                            Debug.LogWarning("Matchmaker status: Failed/Timeout");
                            polling = false;
                            onMatchSearchCompleted?.Invoke(null, new List<(string, int)>());
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Polling error: {ex.Message}");
                    polling = false;
                    onMatchSearchCompleted?.Invoke(null, new List<(string, int)>());
                }
            }

            await StopSearch();
        }

    }
}
