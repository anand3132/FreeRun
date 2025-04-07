using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RedGaint.Network.Runtime
{
    public class LobbyView : View<GameApplication>
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private List<TMP_Text> playerNameSlots;
        
        // internal LobbyView Lobby => m_LobbyView;
        // [SerializeField] 
        // LobbyView m_LobbyView;
        //
        /// <summary>
        /// Updates the countdown text shown in the lobby.
        /// </summary>
        /// <param name="secondsRemaining">How many seconds remain before the match starts.</param>
        public void UpdateCountdown(float secondsRemaining)
        {
            countdownText.text = $"Game starts in {secondsRemaining}...";
        }

        /// <summary>
        /// Populates player name slots in the lobby view.
        /// </summary>
        /// <param name="playerNames">List of player names currently in the lobby.</param>
        public void UpdatePlayerList(List<string> playerNames)
        {
            for (int i = 0; i < playerNameSlots.Count; i++)
            {
                if (i < playerNames.Count)
                    playerNameSlots[i].text = playerNames[i];
                else
                    playerNameSlots[i].text = "Waiting...";
            }
        }

        /// <summary>
        /// Shows the lobby panel.
        /// </summary>
        public void ShowLobby()
        {
            Show();
        }

        /// <summary>
        /// Hides the lobby panel.
        /// </summary>
        public void HideLobby()
        {
            Hide();
        }
    }
}