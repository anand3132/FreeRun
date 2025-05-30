using System.Collections.Generic;
using RedGaint.Network.Runtime.UserData;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedGaint.Network.Runtime
{
    public class LobbyView : View<MetagameApplication>
    {
        [Header("UI Elements")]
        private Label m_TitleLabel;
        private Label m_InfoLabel;
        private Button m_ExitButton;
        private UIDocument m_UIDocument;

        private HashSet<string> displayedPlayerIds = new(); // To prevent duplicates

        void Awake()
        {
            m_UIDocument = GetComponent<UIDocument>();
            var root = m_UIDocument.rootVisualElement;

            m_TitleLabel = root.Q<Label>("timerLabel");
            m_InfoLabel = root.Q<Label>("titleLabel");
            m_ExitButton = root.Q<Button>("quitButton");

            m_TitleLabel.text = GlobalTextBridge.LobbyWaitingText;
        }

        void OnEnable()
        {
            m_ExitButton.clicked += OnClickQuitClicked;
        }

        void OnDisable()
        {
            m_ExitButton.clicked -= OnClickQuitClicked;
        }

        private void OnClickQuitClicked()
        {
            // Optional: add confirmation or debug logic
            Debug.Log("Quit clicked in lobby (optional)");
        }

        /// <summary>
        /// Updates the countdown display.
        /// </summary>
        public void UpdateCountdown(float secondsRemaining)
        {
            m_TitleLabel.text = $"Game starts in {Mathf.CeilToInt(secondsRemaining)}...";
        }

        /// <summary>
        /// Updates the lobby view with all currently connected players.
        /// Shows their character models using Stage.Instance.
        /// </summary>
        public void UpdatePlayerList(List<PlayerData> players)
        {
            foreach (var player in players)
            {
                if (!displayedPlayerIds.Contains(player.PlayerId))
                {
                    var table = Stage.Instance.GetAvailableTable();
                    Stage.Instance.ShowCharacterOnTable(table, player.CharacterId);
                    Debug.Log($"Player joined lobby: {player.PlayerName} with CharacterID: {player.CharacterId}");
                    displayedPlayerIds.Add(player.PlayerId);
                }
            }
        }

        internal override void Show()
        {
            base.Show();
            displayedPlayerIds.Clear(); // Reset state when entering view
        }
    }
}
