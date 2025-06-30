using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Multiplayer;
using RedGaint.Utility;
using Random = UnityEngine.Random;

namespace RedGaint.Network.Runtime
{
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
                return;

            Instance = this;
            DontDestroyOnLoad(gameObject);
            OnConfigurationLoaded();
        }

        async void OnConfigurationLoaded()
        {
            await Initialize(MultiplayerRolesManager.ActiveMultiplayerRoleMask == MultiplayerRoleFlags.Client);
        }

        public async Task Initialize(bool isClient)
        {
            Debug.Log("---------------------------------------------------------------------------------");
            // string serviceProfileName = $"FunRunServerProfile_{Guid.NewGuid()}";
             string serviceProfileName = $"FunRunServerProfile_";
            if (!isClient)
            {
                UnityServices.ExternalUserId = k_ServerID;
                await UnityServiceAuthenticator.TrySignInAsync(k_Environment, serviceProfileName+k_ServerID);
                BugsBunnyLogger.Log("Server Profile Name: " + serviceProfileName);
                BugsBunnyLogger.Log("Environment: " + k_Environment);
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

        public struct SignInResult
        {
            public bool Success;
            public string PlayerId;
            public string Username;
        }

        //Auto login 
        public async Task<SignInResult> InitializeAndSignIn(SignInMethod method, Tuple<string, string> credentials = null)
        {
            var result = new SignInResult { Success = false };

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

                    BugsBunnyLogger.Log($"Signed in with {method}. PlayerID: {AuthenticationService.Instance.PlayerId}");
                }
                else
                {
                    BugsBunnyLogger.Log("Already signed in.");
                }

                result.Success = AuthenticationService.Instance.IsSignedIn;
                result.PlayerId = AuthenticationService.Instance.PlayerId;
                result.Username = AuthenticationService.Instance?.PlayerInfo?.Username;
            }
            catch (AuthenticationException ex)
            {
                BugsBunnyLogger.LogRed($"Authentication failed: {ex.Message}", this);
            }
            catch (RequestFailedException ex)
            {
                BugsBunnyLogger.LogRed($"Unity request failed: {ex.Message}", this);
            }
            catch (Exception ex)
            {
                BugsBunnyLogger.LogRed($"Unexpected error: {ex.Message}", this);
            }

            return result;
        }

        private async Task<bool> TryAutoLogin(Tuple<string, string> credentials = null)
        {
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                    await UnityServices.InitializeAsync();

                string encryptionKey = UserData.CloudPlayerProfileHandler.GetEncryptionKeyFromCloud();

#if UNITY_EDITOR
                bool hasStored = EditorCredentialManager.TryLoadCredentialsFromFile(encryptionKey, out string username, out string password);
#else
                bool hasStored = UserCredentialManager.TryLoadCredentials(encryptionKey, out string username, out string password);
#endif

                if (hasStored)
                {
                    await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                    BugsBunnyLogger.Log($"Auto-login successful. PlayerID: {AuthenticationService.Instance.PlayerId}");
                    return true;
                }

                // Fallback to passed-in credentials
                if (credentials != null)
                {
                    await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(credentials.Item1, credentials.Item2);
                    BugsBunnyLogger.Log($"Credential login successful. PlayerID: {AuthenticationService.Instance.PlayerId}");

#if UNITY_EDITOR
                    EditorCredentialManager.SaveCredentialsToFile(credentials.Item1, credentials.Item2, encryptionKey);
#else
                    UserCredentialManager.SaveCredentials(credentials.Item1, credentials.Item2, encryptionKey);
#endif
                    return true;
                }

                Debug.Log("Login failed: No credentials found or provided.");
                return false;
            }
            catch (AuthenticationException ex)
            {
                BugsBunnyLogger.LogRed($"Auto-login failed: {ex.Message}", this);
                return false;
            }
            catch (RequestFailedException ex)
            {
                BugsBunnyLogger.LogRed($"Request failed: {ex.Message}", this);
                return false;
            }
        }

        public async Task<bool> TrySignUp(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                BugsBunnyLogger.Log("✅ Sign up successful!");
                return true;
            }
            catch (AuthenticationException authEx)
            {
                BugsBunnyLogger.LogRed($"Sign up failed - Authentication error: {authEx.Message}", this);
            }
            catch (RequestFailedException requestEx)
            {
                BugsBunnyLogger.LogRed($"Sign up failed - Request error: {requestEx.Message}", this);
            }

            return false;
        }

        public async Task<bool> TrySignOut()
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
                BugsBunnyLogger.Log("Successfully signed out.");
                return true;
            }

            BugsBunnyLogger.Log("Not signed in; skipping sign-out.");
            return false;
        }

        void InitializeClientOnlyServices()
        {
            Matchmaker = gameObject.AddComponent<MatchmakerTicketer>();
        }

        public bool LogThisClass  => true;
    }
}
