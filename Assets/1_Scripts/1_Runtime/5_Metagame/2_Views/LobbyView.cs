using System.Collections.Generic;
using System.Threading.Tasks;
using RedGaint.Network.Runtime.UserData;
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
        private System.Action quitClickHandler;

        void OnEnable()
        {
            quitClickHandler = () => _ = OnClickQuitClickedAsync();
            m_ExitButton.clicked += quitClickHandler;
            
        }

        void OnDisable()
        {
            m_ExitButton.clicked -= quitClickHandler;
        }

        private async Task OnClickQuitClickedAsync()
        {
            Debug.Log("<color=blue>Allocation Server Logs---------------------------------------</color>");
    
            string log = await CloudModule.Instance.GetAllocationServerLog();
            Debug.Log(log);

            // string lobbyId = CloudModule.Instance.CurrentLobbyId;
            // if (!string.IsNullOrEmpty(lobbyId))
            // {
            //     string result = await CloudModule.Instance.EndGameSessionAsync(lobbyId);
            //     Debug.Log(result);
            // }

            Debug.Log("Quit clicked in lobby");
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
        public void UpdatePlayerList(List<PlayerSummary> players)
        {
            foreach (var player in players)
            {
                if (!displayedPlayerIds.Contains(player.PlayerId))
                {
                    var table = Stage.Instance.GetAvailableTable();
                    Stage.Instance.ShowCharacterOnTable(table, player.SelectedCharacterId);
                    Stage.Instance.UpdateTableUserName(table,player.DisplayName);
                    Debug.Log($"Player joined lobby: {player.DisplayName} with CharacterID: {player.SelectedCharacterId}:name{player.DisplayName}");
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
