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
            //server login handled over here 
            string serviceProfileName = ProfileManager.Singleton.Profile;
            if (!isClient)
            {
                //servers should always have a single ID so their data isn't mixed with Users'.
                UnityServices.ExternalUserId = k_ServerID;
                await UnityServiceAuthenticator.TrySignInAsync(k_Environment, serviceProfileName);
                Debug.Log("Server Profile Name:"+serviceProfileName);
                Debug.Log("Environment :"+k_Environment);

            }
            
            // if (isClient)
            // {
            //     //wait for the MetagameApplication to be instantiated, to avoid race conditions
            //     StartCoroutine(CoroutinesHelper.WaitAndDo(new WaitUntil(() => MetagameApplication.Instance), () =>
            //     {
            //         //MetagameApplication.Instance.Broadcast(new PlayerSignedIn(signedIn, UnityServiceAuthenticator.PlayerId));
            //
            //         //at this point, it's safe to tell the Application that the player signed in
            //         // MetagameApplication.Instance.Broadcast(new PlayerSignedIn(signedIn, UnityServiceAuthenticator.PlayerId));
            //         // if (signedIn)
            //         // {
            //         //     InitializeClientOnlyServices();
            //         // }
            //         // else
            //         // {
            //         //     Debug.LogError("User could not sign in. Please check that your device is connected to the internet, and that the project is linked to an existing Project in the Unity Cloud.");
            //         // }
            //     }));
            // }
        }
        
        public enum SignInMethod
        {
            Anonymous,
            Apple,
            Google,
            Facebook,
            UsernamePassword
        }

        public async Task InitializeAndSignIn(SignInMethod method, string customID = null)
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
                                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(GlobalStaticVariables.UserName,GlobalStaticVariables.Password);
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
        
        void InitializeClientOnlyServices()
        {
            Matchmaker = gameObject.AddComponent<MatchmakerTicketer>();
        }
    }
}
