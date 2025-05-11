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
        
        private int currentStageFocused = -1;
        
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
            if (currentStageFocused==-1)
                currentStageFocused = Stage.Instance.GetAvailableTable();
            UpdateModelView(currentStageFocused,UserProfileManager.CurrentUser.CharacterId);
            Stage.Instance.UpdateTableUserName(currentStageFocused,UserProfileManager.CurrentUser.Username);
        }

        void OnNextClicked()
        {
            Stage.Instance.ShowNextCharacterOnTable(currentStageFocused);
            if (Stage.Instance.TryGetCurrentCharacterOnStage(currentStageFocused, out Character currentCharacter))
            {
                m_ModelNameLabel.text = $"Selected Model: {currentCharacter.DisplayName}";
            }
            IsProfileDirty = true;
        }

        void OnPreviousClicked()
        {
            Stage.Instance.ShowPreviousCharacterOnTable(currentStageFocused);
            if (Stage.Instance.TryGetCurrentCharacterOnStage(currentStageFocused, out Character currentCharacter))
            {
                m_ModelNameLabel.text = $"Selected Model: {currentCharacter.DisplayName}";
            }
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
                if (Stage.Instance.TryGetCurrentCharacterOnStage(currentStageFocused, out Character currentCharacter))
                {
                    UserProfileManager.CurrentUser.CharacterId= currentCharacter.Id;
                }
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

        void UpdateModelView(int stageID, string characterID)
        {
           
            Stage.Instance.ShowCharacterOnTable(currentStageFocused, characterID);
            // Stage.Instance.ShowCharacterOnTable(currentStageFocused++, "2");
            // Stage.Instance.ShowCharacterOnTable(currentStageFocused++, "3");
            // Stage.Instance.ShowCharacterOnTable(currentStageFocused++, "4");
            // Stage.Instance.ShowCharacterOnTable(currentStageFocused++, "1");
            // Stage.Instance.ShowCharacterOnTable(currentStageFocused++, "2");

            Stage.Instance.FocusStage();
           // Stage.Instance.FocusCharacterOnTable(currentStageFocused);
            if (Stage.Instance.TryGetCurrentCharacterOnStage(stageID, out Character currentCharacter))
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
