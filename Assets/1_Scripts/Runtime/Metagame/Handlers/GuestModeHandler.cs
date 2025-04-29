using UnityEngine.UIElements;
using UnityEngine;
namespace RedGaint.Network.Runtime
{
    
    public class GuestMenuHandler : IMenuModeHandler
    {
        private readonly UIReferences _ui;

        public GuestMenuHandler(UIReferences ui) => _ui = ui;

        public void Initialize()
        {
            var name= $"Guest-{SystemInfo.deviceUniqueIdentifier.Substring(0, 6)}";
            UserData.PlayerProfileData guestProfile=GameProfileManager.Instance.CreateGuestProfile();
            _ui.NameLabel.text = guestProfile.Username;
            _ui.ProfileButton.clicked += OnClickProfile;
            
            _ui.ProfileButton.text = GlobalTextBridge.SignInButtonText;
        }

        public void Cleanup()
        {
            _ui.ProfileButton.clicked -= OnClickProfile;
        }

        void OnClickProfile()
        {
            MetagameApplication.Instance.Broadcast(new EnterLoginEvent());
        }
    }


}