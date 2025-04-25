using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedGaint.Network.Runtime
{
    internal class ModelSelectionView : View<MetagameApplication>
    {
        Button m_NextButton;
        Button m_PreviousButton;
        Button m_MainMenuButton;
        Button m_StartMultiplayerButton;
        Label m_ModelNameLabel;
        public GameObject m_Stage;
        
        Character CurrentSelectedCharacter;
        string[] m_ModelNames = { "BotA", "BotB", "BotC", "BotD" };
        int m_CurrentIndex = 0;
        

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_NextButton = root.Q<Button>("nextButton");
            m_PreviousButton = root.Q<Button>("previousButton");
            m_MainMenuButton = root.Q<Button>("mainMenuButton");
            m_StartMultiplayerButton = root.Q<Button>("startMultiplayerButton");
            m_ModelNameLabel = root.Q<Label>("modelNameLabel");
            
            m_NextButton.clicked += OnNextClicked;
            m_PreviousButton.clicked += OnPreviousClicked;
            m_MainMenuButton.clicked += OnBackClicked;
            m_StartMultiplayerButton.clicked += OnStartMultiplayerClicked;
            UpdateModelLabel();
            m_Stage.SetActive(true);
        }

        void OnNextClicked()
        {
            m_CurrentIndex = (m_CurrentIndex + 1) % m_ModelNames.Length;
            UpdateModelLabel();
        }

        void OnPreviousClicked()
        {
            m_CurrentIndex = (m_CurrentIndex - 1 + m_ModelNames.Length) % m_ModelNames.Length;
            UpdateModelLabel();
        }

        void OnBackClicked()
        {
            m_Stage.SetActive(false);
            MetagameApplication.Instance.Broadcast(new EnterMainMenuEvent());
            Debug.Log(App.View.Name());
            App.View.ModelSelectionView.Hide();
        }

        void OnStartMultiplayerClicked()
        {
            // Debug.Log($"Starting multiplayer with model: {m_ModelNames[m_CurrentIndex]}");
            //
            // // Example mapping model name to character ID (update this logic to your actual character system)
            // int selectedCharacterId = m_CurrentIndex;
            //
            // // Show the Waiting Area view and pass callbacks
            // var waitingView = Application.ShowView<WaitingAreaView>(); // Assuming Application is your ViewManager
            //
            // BaseApplication.Instance.MatchmakerTicketer.FindMatch(
            //     queueName: "default", // Replace with your actual queue name
            //     characterId: selectedCharacterId,
            //     onMatchSearchCompleted: waitingView.OnMatchFound,
            //     onMatchmakerTicked: waitingView.OnMatchmakerTicked
            // );
        }

        void UpdateModelLabel()
        {
            m_ModelNameLabel.text = $"Selected Model: {m_ModelNames[m_CurrentIndex]}";
        }

        void OnDestroy()
        {
            m_Stage.SetActive(false);
            m_NextButton.clicked -= OnNextClicked;
            m_PreviousButton.clicked -= OnPreviousClicked;
            m_MainMenuButton.clicked -= OnBackClicked;
            m_StartMultiplayerButton.clicked -= OnStartMultiplayerClicked;
        }
    }
}
