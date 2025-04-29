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
        private Button UserSignUpButton;

        private Action onAnonymousClick;
        private Action onUserSignInClick;
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
            UserSignUpButton = root.Q<Button>("CreateNewButton");

            appleSignInButton = root.Q<Button>("AppleSignInButton");
            googleSignInButton = root.Q<Button>("GoogleSignInButton");
            facebookSignInButton = root.Q<Button>("FacebookSignInButton");

            // Set up actions
            onUserSignInClick = OnSignInClicked;
            
            onAnonymousClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Anonymous);
            onAppleClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Apple);
            onGoogleClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Google);
            onFacebookClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Facebook);

            // Add listeners
            anonymousSignInButton.clicked += onAnonymousClick;
            
            usernameSignInButton.clicked += onUserSignInClick;
            
            appleSignInButton.clicked += onAppleClick;
            googleSignInButton.clicked += onGoogleClick;
            facebookSignInButton.clicked += onFacebookClick;
            
            UserSignUpButton.clicked += SignUpUser;
        }
        
        async void Start()
        {
            await UnityServices.InitializeAsync();
        }

        public void OnDisable()
        {
            if (anonymousSignInButton != null) anonymousSignInButton.clicked -= onAnonymousClick;
            if (usernameSignInButton != null) usernameSignInButton.clicked -= onUserSignInClick;
            
            if (appleSignInButton != null) appleSignInButton.clicked -= onAppleClick;
            if (googleSignInButton != null) googleSignInButton.clicked -= onGoogleClick;
            if (facebookSignInButton != null) facebookSignInButton.clicked -= onFacebookClick;
            if (UserSignUpButton != null) UserSignUpButton.clicked -= SignUpUser;
        }

        private async void AttemptSignIn(UnityServicesInitializer.SignInMethod method)
        {
            statusLabel.text = $"Signing in with {method}...";
            
            if(await UnityServicesInitializer.Instance.InitializeAndSignIn(method))
                UpdateUserProfile( method);
            else
            {
                statusLabel.text = $"Failed to Login using {method}!";
            }
        }
        
        private async void SignUpUser()
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
                string username = usernameField.text;
                if (string.IsNullOrEmpty(passwordField.text))
                {
                    Debug.LogError("Error: Password is empty.");
                    UnityServicesInitializer.Instance.TrySignOut();
                    return;
                }
                UserCredentialManager.SaveCredentials(username, passwordField.text, cloudEncryptionKey);
                await GameProfileManager.Instance.UpdatePlayerProfile(CreateNewUserProfile(username),true);
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("Entered model selection to sign up.");
                    MetagameApplication.Instance.Broadcast(new EnterModelSelectionEvent());
                }            
            }
        }
        
        
        private UserData.PlayerProfileData  CreateNewUserProfile(string username)
        {
            UserData.PlayerProfileData newProfile = new();
            newProfile.PlayerId = AuthenticationService.Instance.PlayerId;
            newProfile.Username = username;
            newProfile.AvatarId = Guid.NewGuid().ToString();
            newProfile.CharacterId = "1";
            newProfile.CurrentLevelId = "Level_01";
            newProfile.XP = 100;
            newProfile.Coins = 100;
            newProfile.ProgressLevel = 0;
            return newProfile;
        }
        
        private async void OnSignInClicked()
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
            UpdateUserProfile(UnityServicesInitializer.SignInMethod.UsernamePassword);
        }

        private async Task UpdateUserProfile(UnityServicesInitializer.SignInMethod method)
        {
            if (Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            {
                string playerID = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
                statusLabel.text = $"Signed in! PlayerID: {playerID}";
                GlobalStaticVariables.UserLoggedInStatus = true;
                string username=null;
                switch (method)
                {
                    case UnityServicesInitializer.SignInMethod.UsernamePassword:
                        username=usernameField.text;
                        break;
                    case UnityServicesInitializer.SignInMethod.Apple:
                        username = SocialLoginHelper.GetAppleUserEmail(GlobalStaticVariables.AppleToken);
                        break;
                    case UnityServicesInitializer.SignInMethod.Google:
                        username = await SocialLoginHelper.GetGoogleUserNameAsync(GlobalStaticVariables.AppleToken);
                        break;
                    case UnityServicesInitializer.SignInMethod.Facebook:
                        username = await SocialLoginHelper.GetFacebookUserNameAsync(GlobalStaticVariables.FacebookToken);
                        break;
                    case UnityServicesInitializer.SignInMethod.Anonymous:
                        username = GameProfileManager.CurrentUser.Username;
                        GameProfileManager.CurrentUser.PlayerId=AuthenticationService.Instance.PlayerId;
                        // GameProfileManager.CurrentUser.AvatarId = string.Empty;
                        // GameProfileManager.CurrentUser.CharacterId = string.Empty;
                        // GameProfileManager.CurrentUser.CurrentLevelId = string.Empty;
                        break;
                }

                if (await GameProfileManager.Instance.LoadAsync(true))
                {
                    MetagameApplication.Instance.Broadcast(new EnterModelSelectionEvent());
                }
                else
                {
                    username=string.IsNullOrEmpty(username)?GameProfileManager.CurrentUser.Username:username;
                    await GameProfileManager.Instance.UpdatePlayerProfile(CreateNewUserProfile(username),true);
                }
                App.View.LoginView.Hide();
            }
        }
    }
}
