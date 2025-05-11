using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedGaint.Network.Runtime
{
    internal class UserProfileView : View<MetagameApplication>
    {
        Button m_NextButton;
        Button m_PreviousButton;
        Button m_MainMenuButton;
        Button m_StartMultiplayerButton;
        Label m_ModelNameLabel;
        
        Character CurrentSelectedCharacter;
        // string[] m_ModelNames = { "BotA", "BotB", "BotC", "BotD" };
        int m_CurrentIndex = 0;
        
        private string stageID = "1";
        
        private bool IsProfileDirty = false;
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
            UpdateModelView(stageID,UserProfileManager.CurrentUser.CharacterId);
            Stage.Instance.UpdateTableUserName(stageID,UserProfileManager.CurrentUser.Username);
            
            UpdateModelView("2","2");
            UpdateModelView("3","3");
            UpdateModelView("4","4");
            UpdateModelView("5","5");

            
        }

        void OnNextClicked()
        {
            Stage.Instance.ShowNextCharacterOnTable(stageID);
            Character currentCharacter = Stage.Instance.GetCurrentCharacterOnStage(stageID);
            m_ModelNameLabel.text = $"Selected Model: {currentCharacter.DisplayName}";
            IsProfileDirty = true;
        }

        void OnPreviousClicked()
        {
            Stage.Instance.ShowPreviousCharacterOnTable(stageID);
            Character currentCharacter = Stage.Instance.GetCurrentCharacterOnStage(stageID);
            m_ModelNameLabel.text = $"Selected Model: {currentCharacter.DisplayName}";
            IsProfileDirty = true;
        }

        void OnBackClicked()
        {
            MetagameApplication.Instance.Broadcast(new EnterMainMenuEvent());
            Debug.Log(App.View.Name());
            App.View.UserProfileView.Hide();
        }

        void OnStartMultiplayerClicked()
        {
            App.View.UserProfileView.Hide();
            if (IsProfileDirty)
            {
                UserProfileManager.CurrentUser.CharacterId = Stage.Instance.GetCurrentCharacterOnStage(stageID).Id;
                //update user profile data
            }
            MetagameApplication.Instance.Broadcast(new EnterLobbyQueueEvent());

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

        void UpdateModelView(string stageID,string characterID)
        {
            Stage.Instance.ShowCharacterOnTable(stageID,characterID);
            Character currentCharacter = Stage.Instance.GetCurrentCharacterOnStage(stageID);
            m_ModelNameLabel.text = $"Selected Model: {currentCharacter.DisplayName}";
        }

        void OnDestroy()
        {
            m_NextButton.clicked -= OnNextClicked;
            m_PreviousButton.clicked -= OnPreviousClicked;
            m_MainMenuButton.clicked -= OnBackClicked;
            m_StartMultiplayerButton.clicked -= OnStartMultiplayerClicked;
        }
    }
}
