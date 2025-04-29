using System;
using System.Threading.Tasks;
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
            AddListener<EnterLobbyQueueEvent>(OnLobbyEntered);
            AddListener<EnterUserProfileEvent>(OnEnterUserProfileEvent);
            AddListener<EnterLoginEvent>(OnEnterLoginEvent);
            AddListener<EnterMainMenuEvent>(OnEnterMainMenuEvent);
            ConnectionManager.EventManager.AddListener<ConnectionEvent>(OnConnectionEvent);
        }
        
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
            RemoveListener<EnterLobbyQueueEvent>(OnLobbyEntered);
            RemoveListener<EnterUserProfileEvent>(OnEnterUserProfileEvent);
            RemoveListener<EnterLoginEvent>(OnEnterLoginEvent);
            RemoveListener<EnterMainMenuEvent>(OnEnterMainMenuEvent);
            ConnectionManager.EventManager.RemoveListener<ConnectionEvent>(OnConnectionEvent);
        }

        private void OnEnterMainMenuEvent(EnterMainMenuEvent evt)
        {
            App.View.MainMenu.Show();
        }
        private void OnEnterLoginEvent(EnterLoginEvent evt)
        { 
            View.Hide();
            App.View.LoginView.Show();
        }

        void OnEnterUserProfileEvent(EnterUserProfileEvent evt)
        {
            View.Hide();
            App.View.ModelSelectionView.Show();
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
