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
    ///<summary>
    /// Holds matchmaker search logic
    ///</summary>
    internal class MatchmakerTicketer : MonoBehaviour
    {
        internal string LastQueueName { get; private set; }
        internal bool Searching { get; private set; }
        private string m_TicketId = "";

        internal async void FindMatch(string queueName, Action<MultiplayAssignment> onMatchSearchCompleted, Action<int> onMatchmakerTicked)
        {
            try
            {
                if (!Searching)
                {
                    if (!string.IsNullOrEmpty(m_TicketId))
                    {
                        Debug.LogError("Already matchmaking!");
                        return;
                    }

                    Searching = true;
                    await StartSearch(queueName, onMatchSearchCompleted, onMatchmakerTicked);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                await StopSearch();
                MetagameApplication.Instance.Broadcast(new ExitMatchmakerQueueEvent());
            }
        }

        private async Task StartSearch(string queueName, Action<MultiplayAssignment> onMatchSearchCompleted, Action<int> onMatchmakerTicked)
        {
            var attributes = new Dictionary<string, object>
            {
                { "platform", "Windows" }
            };
            
            var players = new List<Player>
            {
                new Player(AuthenticationService.Instance.PlayerId, new { }),
            };
            
            var options = new CreateTicketOptions(queueName, attributes);
            var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
            LastQueueName = queueName;
            m_TicketId = ticketResponse.Id;

            _ = PollTicketStatus(onMatchSearchCompleted, onMatchmakerTicked);
        }

        internal async Task StopSearch()
        {
            if (!string.IsNullOrEmpty(m_TicketId))
            {
                await MatchmakerService.Instance.DeleteTicketAsync(m_TicketId);
                m_TicketId = string.Empty;
            }
            Searching = false;
        }

        private async Task PollTicketStatus(Action<MultiplayAssignment> onMatchSearchCompleted, Action<int> onMatchmakerTicked)
        {
            bool polling = true;
            int elapsedTime = 0;

            while (polling)
            {
                try
                {
                    await Task.Delay(1000);
                    elapsedTime++;
                    onMatchmakerTicked?.Invoke(elapsedTime);
                    
                    var response = await MatchmakerService.Instance.GetTicketAsync(m_TicketId);
                    if (response != null && response.Type == typeof(MultiplayAssignment))
                    {
                        MultiplayAssignment assignment = response.Value as MultiplayAssignment;
                        Debug.Log($"<color=red> Got the IP from matchMaking: {assignment.Ip} Port: {assignment.Port}</color>");

                        if (assignment != null)
                        {
                            Debug.Log($"{assignment.Status} <color=red>progress... </color> {assignment.Message}");
                            switch (assignment.Status)
                            {
                                case StatusOptions.InProgress:
                                    // Keep polling
                                    break;
                                case StatusOptions.Found:
                                case StatusOptions.Failed:
                                case StatusOptions.Timeout:
                                    polling = false;
                                    onMatchSearchCompleted?.Invoke(assignment);
                                    break;
                                default:
                                    throw new InvalidOperationException($"Unexpected assignment status: {assignment.Status}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error in polling ticket status: " + e.Message);
                    polling = false;
                    await StopSearch();
                    onMatchSearchCompleted?.Invoke(null);
                }
            }

            await StopSearch();
        }
    }
}
