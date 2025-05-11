using UnityEngine.UIElements;
using UnityEngine;

namespace RedGaint.Network.Runtime
{
    public class UserMenuHandler : IMenuModeHandler
    {
        private readonly UIReferences _ui;

        public UserMenuHandler(UIReferences ui) => _ui = ui;

        public void Initialize()
        {
             _ui.NameLabel.text = "Hello " +UserProfileManager.CurrentUser?.Username ?? "FreeRun";
             Debug.Log("user called");
            _ui.ProfileButton.clicked += OnClickProfile;
            _ui.ProfileButton.text = GlobalTextBridge.ProfileButtonText;
        }

        public void Cleanup()
        {
            _ui.ProfileButton.clicked -= OnClickProfile;
        }

        void OnClickProfile()
        {
            MetagameApplication.Instance.Broadcast(new EnterUserProfileEvent());
        }

    }

}