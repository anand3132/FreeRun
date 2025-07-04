using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedGaint.Network.Runtime
{
    public class SharedUIHandler:IMenuModeHandler
    {
        
        private readonly UIReferences _ui;
        public SharedUIHandler(UIReferences ui)
        {
            _ui = ui;
        }
        public void Initialize()
        {
            _ui.SinglePlayerButton.text = GlobalTextBridge.SinglePlayerButtonText;
            _ui.SinglePlayerButton.clicked += OnSinglePlayerButtonClicked;
            _ui.MultiplayerButton.clicked += OnMultiPlayerButtonClicked;
            _ui.MultiplayerButton.text = GlobalTextBridge.MultiPlayerButtonText;
            _ui.QuitButton.text = GlobalTextBridge.QuitButtonText;
            _ui.QuitButton.clicked += OnQuitButtonClicked;


        }

        public void Cleanup()
        {
            _ui.SinglePlayerButton.clicked -= OnSinglePlayerButtonClicked;
            _ui.MultiplayerButton.clicked -= OnMultiPlayerButtonClicked;
            _ui.QuitButton.clicked -= OnQuitButtonClicked;
        }

        private void OnSinglePlayerButtonClicked()
        {
            MetagameApplication.Instance.Broadcast(new StartSingleplayer());
        }
        private void OnMultiPlayerButtonClicked()
        {
            MetagameApplication.Instance.Broadcast(new EnterLobbyQueueEvent());
        }

        private void OnQuitButtonClicked()
        {
            // fire and forget, but awaited internally
            _ = HandleQuitAsync();
        }

        private async Task HandleQuitAsync()
        {
            bool saveSuccessful = await UserProfileManager.Instance.UpdatePlayerProfile(true);

            // Optional: log or handle if saving failed
            Debug.Log($"Save successful: {saveSuccessful}");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        }
    }


}