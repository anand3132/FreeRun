using UnityEngine;
using UnityEngine.SceneManagement;

namespace RedGaint.Network.Runtime
{
    /// <summary>
    /// Main controller of the <see cref="MetagameApplication"></see>
    /// </summary>
    public class MetagameController : Controller<MetagameApplication>
    {
        void Awake()
        {
            AddListener<PlayerSignedIn>(OnPlayerSignedIn);
            AddListener<MatchEnteredEvent>(OnMatchEntered);
            AddListener<EnterModelSelectionEvent>(OnModelSelection);
            AddListener<StartSingleplayer>(OnSingleGameplaySelection);
        }

        private void OnSingleGameplaySelection(StartSingleplayer obj)
        {
            SceneManager.UnloadSceneAsync(GlobalStaticVariables.MetaScene);
            SceneManager.LoadScene(GlobalStaticVariables.GameScene);
        }

        void OnDestroy()
        {
            RemoveListeners();
        }
        public bool enableAutoLoin = true;

        public  async void Start()
        {
            if (enableAutoLoin)
            {
                if (await UnityServicesInitializer.Instance.InitializeAndSignIn(UnityServicesInitializer.SignInMethod.AutoLogin))
                {
                    await UserProfileManager.Instance.LoadAsync(true);
                    Debug.Log("user loaded");
                }
            }
            App.View.MainMenu.Show();
        }

        internal override void RemoveListeners()
        {
            RemoveListener<PlayerSignedIn>(OnPlayerSignedIn);
            RemoveListener<MatchEnteredEvent>(OnMatchEntered);
            RemoveListener<EnterModelSelectionEvent>(OnModelSelection);
            RemoveListener<StartSingleplayer>(OnSingleGameplaySelection);

        }

        private void OnModelSelection(EnterModelSelectionEvent obj)
        {
            Debug.Log(" model selection Entered.");

            App.View.UserProfileView.Show();
        }

        void OnPlayerSignedIn(PlayerSignedIn evt)
        {
            App.View.MainMenu.Show();
        }

        void OnMatchEntered(MatchEnteredEvent evt)
        {
            DisableViewsAndListeners();
        }

        void DisableViewsAndListeners()
        {
            for (int i = 0; i < App.View.transform.childCount; i++)
            {
                App.View.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
