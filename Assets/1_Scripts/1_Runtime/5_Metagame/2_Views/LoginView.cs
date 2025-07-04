using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedGaint.Network.Runtime
{
    public class LoginView : View<MetagameApplication>
    {
        private Label statusLabel;
        private TextField usernameField;
        // private TextField passwordField;

        private Button anonymousSignInButton;
        private Button usernameSignInButton;
        private Button appleSignInButton;
        private Button googleSignInButton;
        private Button facebookSignInButton;
        private Button UserSignUpButton;
        private Button MainMenuButton;
        
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
            // passwordField = root.Q<TextField>("PasswordField");
            
            anonymousSignInButton = root.Q<Button>("AnonymousSignInButton");
            usernameSignInButton = root.Q<Button>("UsernameSignInButton");
            UserSignUpButton = root.Q<Button>("CreateNewButton");

            appleSignInButton = root.Q<Button>("AppleSignInButton");
            googleSignInButton = root.Q<Button>("GoogleSignInButton");
            facebookSignInButton = root.Q<Button>("FacebookSignInButton");
            MainMenuButton = root.Q<Button>("MainMenuButton");


            // Set up actions
            onUserSignInClick = OnSignInClicked;
            
            onAnonymousClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Anonymous);
            onAppleClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Apple);
            onGoogleClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Google);
            onFacebookClick = () => AttemptSignIn(UnityServicesInitializer.SignInMethod.Facebook);

            // Add listeners
            anonymousSignInButton.clicked += onAnonymousClick;
            MainMenuButton.clicked += OnMainMenuClicked;
            usernameSignInButton.clicked += onUserSignInClick;
            
            appleSignInButton.clicked += onAppleClick;
            googleSignInButton.clicked += onGoogleClick;
            facebookSignInButton.clicked += onFacebookClick;
            
            UserSignUpButton.clicked += SignUpUser;
        }

        private void OnMainMenuClicked()
        {
            App.View.MainMenu.Show();
            App.View.LoginView.Hide();
        }

        async void Start()
        {
#if UNITY_EDITOR
            await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName(UnityServicesInitializer.k_Environment));
#else

            await UnityServices.InitializeAsync();
#endif
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
        // private UserData.PlayerProfileData  CreateNewUserProfile(string username)
        // {
        //     UserData.PlayerProfileData newProfile = new();
        //     newProfile.PlayerId = AuthenticationService.Instance.PlayerId;
        //     newProfile.Username = username;
        //     newProfile.AvatarId = Guid.NewGuid().ToString();
        //     newProfile.CharacterId = "1";
        //     newProfile.CurrentLevelId = "Level_01";
        //     newProfile.XP = 100;
        //     newProfile.Coins = 100;
        //     newProfile.ProgressLevel = 0;
        //     return newProfile;
        // }
        private async void AttemptSignIn(UnityServicesInitializer.SignInMethod method)
        {
            statusLabel.text = $"Signing in with {method}...";
            var result = await UnityServicesInitializer.Instance.InitializeAndSignIn(method);
            if(result.Success)
                UpdateUserProfile( method,result.Username);
            else
            {
                statusLabel.text = $"Failed to Login using {method}!";
            }
        }
        
        private async void SignUpUser()
        {
            bool status = false;
            
            //if (!string.IsNullOrEmpty(usernameField.text) && !string.IsNullOrEmpty(passwordField.text))
            if (!string.IsNullOrEmpty(usernameField.text))

            {
                statusLabel.text = $"Creating a new user...";
                status = await UnityServicesInitializer.Instance.TrySignUp(usernameField.text, GlobalStaticVariables.UserDeafultPassword);
            }
            else
            {
                statusLabel.text = $"Error: Username or Password is empty.";
            }

            if (status)
            {
                string cloudEncryptionKey = UserData.CloudPlayerProfileHandler.GetEncryptionKeyFromCloud();
                string username = usernameField.text;
                // if (string.IsNullOrEmpty(passwordField.text))
                // {
                //     Debug.LogError("Error: Password is empty.");
                //     UnityServicesInitializer.Instance.TrySignOut();
                //     return;
                // }
#if UNITY_EDITOR
                EditorCredentialManager.SaveCredentialsToFile(username,GlobalStaticVariables.UserDeafultPassword, cloudEncryptionKey);
#else
                UserCredentialManager.SaveCredentials(username, passwordField.text, cloudEncryptionKey);
#endif
                UserProfileManager.Instance.CreateNewUserProfile(username,AuthenticationService.Instance.PlayerId);
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("Entered model selection to sign up.");
                    MetagameApplication.Instance.Broadcast(new EnterModelSelectionEvent());
                    App.View.LoginView.Hide();
                }
            }
            else
            {
                statusLabel.text = $"{usernameField.text} is blocked..!!";
            }
        }
        
        
        private async void OnSignInClicked()
        {
            string username = usernameField.value;
            //string password = passwordField.value;
            string password = GlobalStaticVariables.UserDeafultPassword;

            if (string.IsNullOrWhiteSpace(username) )//|| string.IsNullOrWhiteSpace(password))
            {
                statusLabel.text = "Please enter a username";
                return;
            }

            statusLabel.text = $"Signing in {username}...";
            UnityServicesInitializer.SignInResult result=await UnityServicesInitializer.Instance.InitializeAndSignIn(UnityServicesInitializer.SignInMethod.UsernamePassword,new Tuple<string, string>(username,password));
            if(result.Success)
                UpdateUserProfile(UnityServicesInitializer.SignInMethod.UsernamePassword, result.Username);
            else
            {
                SignUpUser();
            }
        }

        private async Task UpdateUserProfile(UnityServicesInitializer.SignInMethod method,string username)
        {
            if (Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            {
                string playerID = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
                statusLabel.text = $"Signed in! PlayerID: {playerID}";
                GlobalStaticVariables.UserLoggedInStatus = true;
                // string username=null;
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
                        username = UserProfileManager.CurrentUser.Username;
                        UserProfileManager.CurrentUser.PlayerId=AuthenticationService.Instance.PlayerId;
                        
                        // GameProfileManager.CurrentUser.AvatarId = string.Empty;
                        // GameProfileManager.CurrentUser.CharacterId = string.Empty;
                        // GameProfileManager.CurrentUser.CurrentLevelId = string.Empty;
                        break;
                }
                //if loading failed create new profile
                if (!await UserProfileManager.Instance.LoadAsync(true,username))
                {
                    UserProfileManager.Instance.CreateNewUserProfile(username,AuthenticationService.Instance.PlayerId);
                    await UserProfileManager.Instance.UpdatePlayerProfile(true);
                }
                MetagameApplication.Instance.Broadcast(new EnterModelSelectionEvent());

                App.View.LoginView.Hide();
            }
        }
    }
}
