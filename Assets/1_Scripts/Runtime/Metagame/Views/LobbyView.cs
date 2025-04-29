using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;


namespace RedGaint.Network.Runtime
{
    public class LobbyView : View<MetagameApplication>
    {
        [Header("UI Elements")]
       // [SerializeField] private List<TMP_Text> playerNameSlots;
        // private Button m_RunButton;
        
        private Label m_TitleLabel;
        private Label m_InfoLabel;

        private bool isOnRun=false;
        private UIDocument m_UIDocument;
        private Button m_ExitButton;
        void Awake()
        {
            m_UIDocument = GetComponent<UIDocument>();
            var root = m_UIDocument.rootVisualElement;
            m_ExitButton = root.Q<Button>("quitButton");
            m_TitleLabel = root.Query<Label>("timerLabel");
            m_InfoLabel = root.Query<Label>("titleLabel");
            m_TitleLabel.text = GlobalTextBridge.LobbyWaitingText;
        }

        void OnEnable()
        {
            m_ExitButton.clicked += OnClickQuitClicked;
        }

        private void OnClickQuitClicked()
        {
        }

        private void OnDisable()
        {
            m_ExitButton.clicked -= OnClickQuitClicked;
        }

        // private void OnClickQuitClicked(ClickEvent evt)
        // {
        //     // if (!isOnRun)
        //     // {
        //     //     // m_TitleLabel.text = "Searching...";
        //     //     // m_RunButton.text = "cancel";
        //     //     isOnRun = true;
        //     //     Broadcast(new LobbyStartedEvent(30f));
        //     //     
        //     // }
        //     // else
        //     // {
        //     //     m_TitleLabel.text = "Searching...";
        //     // }
        // }

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
            m_TitleLabel.text = $"Game starts in {secondsRemaining}...";
        }

        /// <summary>
        /// Populates player name slots in the lobby view.
        /// </summary>
        /// <param name="playerNames">List of player names currently in the lobby.</param>
        public void UpdatePlayerList(List<string> playerNames)
        {
            // for (int i = 0; i < playerNameSlots.Count; i++)
            // {
            //     if (i < playerNames.Count)
            //         playerNameSlots[i].text = playerNames[i];
            //     else
            //         playerNameSlots[i].text = "Waiting...";
            // }
        }

        /// <summary>
        /// Shows the lobby panel.
        /// </summary>
        // public void ShowLobby()
        // {
        //     Show();
        // }
        //
        // /// <summary>
        // /// Hides the lobby panel.
        // /// </summary>
        // public void HideLobby()
        // {
        //     Hide();
        // }
    }
}