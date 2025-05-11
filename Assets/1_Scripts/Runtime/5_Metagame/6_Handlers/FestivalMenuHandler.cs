using UnityEngine;
using UnityEngine.UIElements;
namespace RedGaint.Network.Runtime
{
    public class FestivalMenuHandler : IMenuModeHandler
    {
        private readonly UIReferences _ui;

        public FestivalMenuHandler(UIReferences ui)
        {
            _ui = ui;
        }

        public void Initialize()
        {
            _ui.NameLabel.text = "🎉 Festival Mode 🎉";
            _ui.SinglePlayerButton.text = "Festival Run";
            
            _ui.MultiplayerButton.style.display = DisplayStyle.None; // hide
            _ui.ProfileButton.style.display = DisplayStyle.Flex; // show if needed

            _ui.SinglePlayerButton.clicked += OnFestivalPlay;
            _ui.ProfileButton.clicked += OnModelSelect;
            _ui.QuitButton.clicked += OnQuit;
        }

        public void Cleanup()
        {
            _ui.SinglePlayerButton.clicked -= OnFestivalPlay;
            _ui.ProfileButton.clicked -= OnModelSelect;
            _ui.QuitButton.clicked -= OnQuit;
        }

        void OnFestivalPlay()
        {
            Debug.Log("🎯 Festival Mode - Start Playing!");
            // Broadcast(new EnterFestivalEvent()); // if you have one
        }

        void OnModelSelect() => Debug.Log("Festival Mode - Model Selection");

        void OnQuit() => Application.Quit();
    }

}