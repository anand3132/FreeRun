using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Multiplayer;
using UnityEngine;
using Unity.Services.Authentication;
using System;
using RedGaint.Utility;

namespace RedGaint.Network.Runtime
{
    ///<summary>
    ///Initializes all the Unity Services managers
    ///</summary>
    [MultiplayerRoleRestricted]
    internal class UnityServicesInitializer : MonoBehaviour,IBugsBunny
    {
        public const string k_ServerID = "SERVER";
        public static UnityServicesInitializer Instance { get; private set; }
        public MatchmakerTicketer Matchmaker { get; private set; }
        

        public const string k_Environment =
#if LIVE
                                        "production";
#elif STAGE
                                        "staging";
#else
                                        "development";
#endif
        public void Awake()
        {
            if (Instance && Instance != this)
            {
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            OnConfigurationLoaded();
        }

        async void OnConfigurationLoaded()
        {
            await Initialize(MultiplayerRolesManager.ActiveMultiplayerRoleMask == MultiplayerRoleFlags.Client);
        }

        async public Task Initialize(bool isClient)
        {
            string serviceProfileName = $"FunRunServerProfile-{Guid.NewGuid()}";
            if (!isClient)
            {
                //servers should always have a single ID so their data isn't mixed with Users'.
                UnityServices.ExternalUserId = k_ServerID;
                await UnityServiceAuthenticator.TrySignInAsync(k_Environment, serviceProfileName);
                BugsBunny.Log("Server Profile Name:"+serviceProfileName);
                BugsBunny.Log("Environment :"+k_Environment);

            }
        }
        
        public enum SignInMethod
        {
            Anonymous,
            Apple,
            Google,
            Facebook,
            UsernamePassword,
            AutoLogin,
            None
        }

        public async Task<bool> InitializeAndSignIn(SignInMethod method, Tuple<string,string> credentials=null)
        {
            try
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    switch (method)
                    {
                        case SignInMethod.Anonymous:
                            await AuthenticationService.Instance.SignInAnonymouslyAsync();
                            break;

                        case SignInMethod.Apple:
                            await AuthenticationService.Instance.SignInWithAppleAsync(GlobalStaticVariables.AppleToken);
                            break;

                        case SignInMethod.Google:
                            await AuthenticationService.Instance.SignInWithGoogleAsync(GlobalStaticVariables.GoolgeToken);
                            break;

                        case SignInMethod.Facebook:
                            await AuthenticationService.Instance.SignInWithFacebookAsync(GlobalStaticVariables.FacebookToken);
                            break;

                        case SignInMethod.UsernamePassword:
                            await TryAutoLogin(credentials);
                            break;
                        case SignInMethod.AutoLogin:
                            await TryAutoLogin();
                            break;
                    }

                    BugsBunny.Log($"Signed in with {method}. PlayerID: {AuthenticationService.Instance.PlayerId}");
                }
                else
                {
                    BugsBunny.Log("Already signed in.");
                }
            }
            catch (AuthenticationException ex)
            {
                BugsBunny.LogRed($"Authentication failed: {ex.Message}",this);
                return false;
            }
            catch (RequestFailedException ex)
            {
                BugsBunny.LogRed($"Unity request failed: {ex.Message}",this);
                return false;
            }
            catch (Exception ex)
            {
                BugsBunny.LogRed($"Unexpected error: {ex.Message}",this);
                return false;
            }
            return true;
        }
        private async Task<bool> TryAutoLogin(Tuple<string,string> credentials=null)
        {
            try
            {
                // Initialize Unity Services if not already done
                if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
                {
                    await UnityServices.InitializeAsync();
                }

                // Get your encryption key (you should load this from Unity Cloud Save or secure backend)
                string encryptionKey =  UserData.CloudPlayerProfileHandler.GetEncryptionKeyFromCloud(); // Replace with actual implementation

                // Try to load stored credentials
                bool loginState = false;
#if UNITY_EDITOR
                loginState = EditorCredentialManager.TryLoadCredentialsFromFile(encryptionKey, out string username, out string password);
#else
                loginState = UserCredentialManager.TryLoadCredentials(encryptionKey, out string username, out string password);
#endif
                    
                if (loginState)
                {
                    // Attempt to sign in with stored credentials
                    await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                    BugsBunny.Log(" Auto-login successful."+AuthenticationService.Instance.AccessToken);
                    return true;
                }
                else
                {
                    BugsBunny.Log(" No credentials stored or decryption failed.");
                    if (credentials != null)
                    {
                        // Attempt to sign in with stored credentials
                        await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(credentials.Item1,
                            credentials.Item2);
                        BugsBunny.Log(" Auto-login successful." + AuthenticationService.Instance.AccessToken);
#if UNITY_EDITOR
                        EditorCredentialManager.SaveCredentialsToFile(username,credentials.Item1, credentials.Item2);
#else
                        UserCredentialManager.SaveCredentials(username, passwordField.text, cloudEncryptionKey);
#endif

                        return true;
                    }
                    BugsBunny.Log("Login failed : ");
                    return false;
                }
            }
            catch (AuthenticationException ex)
            {
                BugsBunny.LogRed($" Auto-login failed: {ex.Message}",this);
                return false;
            }
            catch (RequestFailedException ex)
            {
                BugsBunny.LogRed($" Request failed: {ex.Message}",this);
                return false;
            }
        }
        
        public async Task<bool> TrySignUp(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                BugsBunny.Log("✅ Sign up successful!");
                return true; // success
            }
            catch (AuthenticationException authEx)
            {
                BugsBunny.LogRed($" Sign up failed - Authentication error: {authEx.Message}",this);
            }
            catch (RequestFailedException requestEx)
            {
                BugsBunny.LogRed($"Sign up failed - Request error: {requestEx.Message}",this);
            }

            return false; // failed
        }

        public async Task<bool> TrySignOut()
        {
            return true;
        }
        
        void InitializeClientOnlyServices()
        {
            Matchmaker = gameObject.AddComponent<MatchmakerTicketer>();
        }

        public bool LogThisClass { get; } = true;
    }
}
