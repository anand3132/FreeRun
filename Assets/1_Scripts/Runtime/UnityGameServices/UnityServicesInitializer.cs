using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Multiplayer;
using UnityEngine;
using Unity.Services.Authentication;
using System;

namespace RedGaint.Network.Runtime
{
    ///<summary>
    ///Initializes all the Unity Services managers
    ///</summary>
    [MultiplayerRoleRestricted]
    internal class UnityServicesInitializer : MonoBehaviour
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
                Debug.Log("Server Profile Name:"+serviceProfileName);
                Debug.Log("Environment :"+k_Environment);

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

        public async Task<bool> InitializeAndSignIn(SignInMethod method, string customID = null)
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
                            await TryAutoLogin();
                            break;
                        case SignInMethod.AutoLogin:
                            await TryAutoLogin();
                            break;
                    }

                    Debug.Log($"Signed in with {method}. PlayerID: {AuthenticationService.Instance.PlayerId}");
                }
                else
                {
                    Debug.Log("Already signed in.");
                }
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($"Authentication failed: {ex.Message}");
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError($"Unity request failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex.Message}");
            }

            // if (AuthenticationService.Instance.IsSignedIn)
            // {
            //     MetagameApplication.Instance.Broadcast(new PlayerSignedIn(AuthenticationService.Instance.IsSignedIn,AuthenticationService.Instance.PlayerId,method));
            //     return true;
            // }

            return false;
        }
        private async Task<bool> TryAutoLogin()
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
                if (UserCredentialManager.TryLoadCredentials(encryptionKey, out string username, out string password))
                {
                    // Attempt to sign in with stored credentials
                    await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                    Debug.Log(" Auto-login successful."+AuthenticationService.Instance.AccessToken);
                    return true;
                }
                else
                {
                    Debug.Log(" No credentials stored or decryption failed.");
                    return false;
                }
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($" Auto-login failed: {ex.Message}");
                return false;
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError($" Request failed: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> TrySignUp(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                Debug.Log("✅ Sign up successful!");
                return true; // success
            }
            catch (AuthenticationException authEx)
            {
                Debug.LogError($" Sign up failed - Authentication error: {authEx.Message}");
            }
            catch (RequestFailedException requestEx)
            {
                Debug.LogError($"Sign up failed - Request error: {requestEx.Message}");
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
    }
}
