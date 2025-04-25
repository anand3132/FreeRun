using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedGaint.Network.Runtime
{
    public class LoginView : View<MetagameApplication>
    {
        private Label statusLabel;
        private TextField usernameField;
        private TextField passwordField;

        private Button anonymousSignInButton;
        private Button usernameSignInButton;
        private Button appleSignInButton;
        private Button googleSignInButton;
        private Button facebookSignInButton;
        private Button createNewButton;

        private Action onAnonymousClick;
        private Action onUsernameClick;
        private Action onAppleClick;
        private Action onGoogleClick;
        private Action onFacebookClick;

        public void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // Fetch elements by name (ensure these match your UXML names)
            statusLabel = root.Q<Label>("StatusLabel");
            usernameField = root.Q<TextField>("UsernameField");
            passwordField = root.Q<TextField>("PasswordField");
            
            anonymousSignInButton = root.Q<Button>("AnonymousSignInButton");
            usernameSignInButton = root.Q<Button>("UsernameSignInButton");
            createNewButton = root.Q<Button>("CreateNewButton");

            appleSignInButton = root.Q<Button>("AppleSignInButton");
            googleSignInButton = root.Q<Button>("GoogleSignInButton");
            facebookSignInButton = root.Q<Button>("FacebookSignInButton");

            // Set up actions
            onAnonymousClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Anonymous);
            onUsernameClick = AttemptUsernamePasswordSignIn;
            
            onAppleClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Apple);
            onGoogleClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Google);
            onFacebookClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Facebook);

            // Add listeners
            anonymousSignInButton.clicked += onAnonymousClick;
            usernameSignInButton.clicked += onUsernameClick;
            appleSignInButton.clicked += onAppleClick;
            googleSignInButton.clicked += onGoogleClick;
            facebookSignInButton.clicked += onFacebookClick;
            createNewButton.clicked += CreateNewUser;
        }
        
        async void Start()
        {
            await UnityServices.InitializeAsync();
        }

        public void OnDisable()
        {
            if (anonymousSignInButton != null) anonymousSignInButton.clicked -= onAnonymousClick;
            if (usernameSignInButton != null) usernameSignInButton.clicked -= onUsernameClick;
            
            if (appleSignInButton != null) appleSignInButton.clicked -= onAppleClick;
            if (googleSignInButton != null) googleSignInButton.clicked -= onGoogleClick;
            if (facebookSignInButton != null) facebookSignInButton.clicked -= onFacebookClick;
            if (createNewButton != null) createNewButton.clicked -= CreateNewUser;
        }

        private async void AttemptSignIn(UnityServicesInitializer.SignInMethod method)
        {
            statusLabel.text = $"Signing in with {method}...";
            await UnityServicesInitializer.Instance.InitializeAndSignIn(method);
            UpdateStatus();
        }

        private async void CreateNewUser()
        {
            bool status = false;
            if (!string.IsNullOrEmpty(usernameField.text) && !string.IsNullOrEmpty(passwordField.text))
            {
                statusLabel.text = $"Creating a new user...";
                status = await UnityServicesInitializer.Instance.TrySignUp(usernameField.text, passwordField.text);
            }
            else
            {
                statusLabel.text = $"Error: Username or Password is empty.";
            }

            if (status)
            {
                string cloudEncryptionKey = UserData.CloudPlayerProfileHandler.GetEncryptionKeyFromCloud();
                UserCredentialManager.SaveCredentials(usernameField.text, passwordField.text, cloudEncryptionKey);
                string username = usernameField.text;
                GameProfileManager.SaveAsync(false,CreateNewUserProfile(username));
            }
        }

        private UserData.PlayerProfileData CreateNewUserProfile(string username)
        {
            UserData.PlayerProfileData newProfile = new();
            newProfile.PlayerId= Guid.NewGuid().ToString();
            newProfile.Username = username;
            newProfile.AvatarId= Guid.NewGuid().ToString();
            newProfile.CurrentSelectedCharacterId = "1";
            newProfile.CurrentSelectedLevelId = "Level_01";
            newProfile.XP = 100;
            newProfile.Coins = 100;
            newProfile.ProgressLevel = 0;
            return newProfile;
        }
        private async void AttemptUsernamePasswordSignIn()
        {
            string username = usernameField.value;
            string password = passwordField.value;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                statusLabel.text = "Please enter a username and password.";
                return;
            }

            statusLabel.text = "Signing in with username & password...";
            await UnityServicesInitializer.Instance.InitializeAndSignIn(UnityServicesInitializer.SignInMethod.UsernamePassword);
            UpdateStatus();
        }
        


        private void UpdateStatus()
        {
            if (Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            {
                string playerID = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
                statusLabel.text = $"Signed in! PlayerID: {playerID}";
                GlobalStaticVariables.UserLoggedInStatus = true;
                MetagameApplication.Instance.Broadcast(new PlayerSignedIn(GlobalStaticVariables.UserLoggedInStatus, playerID));
            }
            else
            {
                statusLabel.text = "Sign-in failed. Check credentials or try again.";
            }
        }
    }
}
