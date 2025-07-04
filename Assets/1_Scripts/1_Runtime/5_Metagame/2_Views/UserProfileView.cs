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
            m_MainMenuButton.clicked += OnMainMenuClicked;
            m_StartMultiplayerButton.clicked += OnStartMultiplayerClicked;
            if (currentStageFocused==-1)
                currentStageFocused = Stage.Instance.GetAvailableTable();
            UpdateModelView(currentStageFocused,UserProfileManager.CurrentUser.CharacterId);
            Stage.Instance.UpdateTableUserName(currentStageFocused,UserProfileManager.CurrentUser.Username);
        }

        void OnNextClicked()
        {
            if (Stage.Instance.TryGetCurrentCharacterOnStage(currentStageFocused, out Character currentCharacter))
            {
                Stage.Instance.ShowNextCharacterOnTable(currentStageFocused, currentCharacter.Id);
                Stage.Instance.TryGetCurrentCharacterOnStage(currentStageFocused, out var nextCharacter);
                m_ModelNameLabel.text = $"Selected Model: {nextCharacter.DisplayName}";
                IsProfileDirty = true;
            }
        }


        void OnPreviousClicked()
        {
            if (Stage.Instance.TryGetCurrentCharacterOnStage(currentStageFocused, out Character currentCharacter))
            {
                Stage.Instance.ShowPreviousCharacterOnTable(currentStageFocused, currentCharacter.Id);
                Stage.Instance.TryGetCurrentCharacterOnStage(currentStageFocused, out var previousCharacter);
                m_ModelNameLabel.text = $"Selected Model: {previousCharacter.DisplayName}";
                IsProfileDirty = true;
            }
        }
        void OnMainMenuClicked()
        {
            MetagameApplication.Instance.Broadcast(new EnterMainMenuEvent());
            Debug.Log(App.View.Name());
            if(Stage.Instance.TryGetCurrentCharacterOnStage(currentStageFocused, out var characterOnStage)){
                UserProfileManager.CurrentUser.CharacterId = characterOnStage.Id;
                UserProfileManager.Instance.UpdatePlayerProfile(false);
            }
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
            var characters = Stage.Instance.characterDatabase.GetAllCharacters();
            int characterIndex = Array.FindIndex(characters, c => c.Id == characterID);
    
            if (characterIndex == -1)
            {
                Debug.LogWarning($"Character ID '{characterID}' not found in character database. Defaulting to first character.");
                characterIndex = 0;
            }

            // Set the index mapping correctly
            if (!Stage.Instance.currentCharacterIndexes.ContainsKey(stageID))
            {
                Stage.Instance.currentCharacterIndexes[stageID] = characterIndex;
            }
            else
            {
                Stage.Instance.currentCharacterIndexes[stageID] = characterIndex;
            }

            Stage.Instance.ShowCharacterOnTable(stageID, characters[characterIndex].Id);
            Stage.Instance.FocusStage();

            if (Stage.Instance.TryGetCurrentCharacterOnStage(stageID, out Character currentCharacter))
                m_ModelNameLabel.text = $"Selected Model: {currentCharacter.DisplayName}";
        }


        void OnDestroy()
        {
            m_NextButton.clicked -= OnNextClicked;
            m_PreviousButton.clicked -= OnPreviousClicked;
            m_MainMenuButton.clicked -= OnMainMenuClicked;
            m_StartMultiplayerButton.clicked -= OnStartMultiplayerClicked;
        }
    }
}
