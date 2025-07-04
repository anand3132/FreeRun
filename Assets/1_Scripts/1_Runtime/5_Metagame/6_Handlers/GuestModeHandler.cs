using UnityEngine;

namespace RedGaint.Network.Runtime
{
    public class GuestMenuHandler : IMenuModeHandler
    {
        private readonly UIReferences _ui;

        public GuestMenuHandler(UIReferences ui) => _ui = ui;

        public void Initialize()
        {
            UserData.PlayerProfileData guestProfile;

#if UNITY_EDITOR
            string randomId = Random.Range(0, 9999).ToString("D4"); // Padded: 0001–9999
            guestProfile = UserProfileManager.Instance.CreateNewUserProfile("Guest_" + randomId,"GuistID",true);
#else
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(deviceId) || deviceId.Length < 6)
            {
                deviceId = System.Guid.NewGuid().ToString("N"); // fallback
            }
            guestProfile = UserProfileManager.Instance.CreateNewUserProfile("Guest_" + deviceId[..6],"GuistID",true);
#endif

            _ui.NameLabel.text = guestProfile.Username;
            _ui.ProfileButton.text = GlobalTextBridge.SignInButtonText;
            _ui.ProfileButton.clicked += OnClickProfile;
        }

        public void Cleanup()
        {
            _ui.ProfileButton.clicked -= OnClickProfile;
        }

        private void OnClickProfile()
        {
            MetagameApplication.Instance.Broadcast(new EnterLoginEvent());
        }
    }
}