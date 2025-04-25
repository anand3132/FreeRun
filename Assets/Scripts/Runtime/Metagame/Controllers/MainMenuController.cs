using System;
using RedGaint.Network.Runtime.ApplicationLifecycle;
using RedGaint.Network.Runtime.ConnectionManagement;
using UnityEngine;

namespace RedGaint.Network.Runtime
{
    internal class MainMenuController : Controller<MetagameApplication>
    {
        MainMenuView View => App.View.MainMenu;
        ConnectionManager ConnectionManager => ApplicationEntryPoint.Singleton.ConnectionManager;

        void Awake()
        {
            AddListener<EnterMatchmakerQueueEvent>(OnEnterMatchmakerQueue);
            AddListener<ExitMatchmakerQueueEvent>(OnExitMatchmakerQueue);
            AddListener<EnterIPConnectionEvent>(OnEnterIPConnection);
            AddListener<ExitIPConnectionEvent>(OnExitIPConnection);
            AddListener<EnterUserProfileEvent>(OnEnterUserProfileEvent);
            AddListener<EnterLoginEvent>(OnEnterLoginEvent);
            ConnectionManager.EventManager.AddListener<ConnectionEvent>(OnConnectionEvent);
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey("username") && PlayerPrefs.HasKey("password"))
            {
                string savedUsername = PlayerPrefs.GetString("username");
                string savedPassword = PlayerPrefs.GetString("password");

                GlobalStaticVariables.UserName = savedUsername;
                GlobalStaticVariables.Password = savedPassword;

                _ = UnityServicesInitializer.Instance.InitializeAndSignIn(UnityServicesInitializer.SignInMethod.UsernamePassword);
            }
            else
            {
                
                Debug.Log("Show main Menu....");
                App.View.MainMenu.Show();
            }
        }

        // private void OnLoginEntered()
        // {
        //     App.View.Login.Show();
        //     View.Hide();
        // }

        private void OnLobbyEntered(EnterLobbyQueueEvent obj)
        {
            View.Hide();
        }
        void OnDestroy()
        {
            RemoveListeners();
        }

        internal override void RemoveListeners()
        {
            RemoveListener<EnterMatchmakerQueueEvent>(OnEnterMatchmakerQueue);
            RemoveListener<ExitMatchmakerQueueEvent>(OnExitMatchmakerQueue);
            RemoveListener<EnterIPConnectionEvent>(OnEnterIPConnection);
            RemoveListener<ExitIPConnectionEvent>(OnExitIPConnection);
            RemoveListener<EnterLobbyQueueEvent>(OnLobbyEntered);
            RemoveListener<EnterUserProfileEvent>(OnEnterUserProfileEvent);
            RemoveListener<EnterLoginEvent>(OnEnterLoginEvent);

            ConnectionManager.EventManager.RemoveListener<ConnectionEvent>(OnConnectionEvent);
        }

        private void OnEnterLoginEvent(EnterLoginEvent obj)
        { 
            View.Hide();
            App.View.LoginView.Show();
        }

        void OnEnterUserProfileEvent(EnterUserProfileEvent evt)
        {
            View.Hide();
            App.View.ModelSelectionView.Show();
        }

        void OnEnterMatchmakerQueue(EnterMatchmakerQueueEvent evt)
        {
            View.Hide();
        }

        void OnExitMatchmakerQueue(ExitMatchmakerQueueEvent evt)
        {
            View.Show();
        }

        void OnEnterIPConnection(EnterIPConnectionEvent evt)
        {
            View.Hide();
        }

        void OnExitIPConnection(ExitIPConnectionEvent evt)
        {
            View.Show();
        }

        void OnConnectionEvent(ConnectionEvent evt)
        {
            switch (evt.status)
            {
                case ConnectStatus.Success:
                case ConnectStatus.ServerFull:
                case ConnectStatus.IncompatibleVersions:
                case ConnectStatus.UserRequestedDisconnect:
                case ConnectStatus.GenericDisconnect:
                case ConnectStatus.ServerEndedSession:
                case ConnectStatus.StartClientFailed:
                    View.Show();
                    break;
            }
        }
    }
}
