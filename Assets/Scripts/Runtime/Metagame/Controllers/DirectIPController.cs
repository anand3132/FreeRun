using System;
using RedGaint.Network.Runtime.ApplicationLifecycle;
using RedGaint.Network.Runtime.ConnectionManagement;
using UnityEngine;

namespace RedGaint.Network.Runtime
{
    internal class DirectIPController : Controller<MetagameApplication>
    {
        DirectIPView View => App.View.DirectIP;
        ConnectionManager ConnectionManager => ApplicationEntryPoint.Singleton.ConnectionManager;

        void Awake()
        {
            AddListener<EnterIPConnectionEvent>(OnEnterIPConnection);
            AddListener<ExitIPConnectionEvent>(OnExitIPConnection);
            AddListener<JoinThroughDirectIPEvent>(OnJoinGame);
            ConnectionManager.EventManager.AddListener<ConnectionEvent>(OnConnectionEvent);
        }

        void OnDestroy()
        {
            RemoveListeners();
        }

        internal override void RemoveListeners()
        {
            RemoveListener<EnterIPConnectionEvent>(OnEnterIPConnection);
            RemoveListener<ExitIPConnectionEvent>(OnExitIPConnection);
            RemoveListener<JoinThroughDirectIPEvent>(OnJoinGame);
            ConnectionManager.EventManager.RemoveListener<ConnectionEvent>(OnConnectionEvent);
        }

        void OnEnterIPConnection(EnterIPConnectionEvent evt)
        {
            View.Show();
        }

        void OnExitIPConnection(ExitIPConnectionEvent evt)
        {
            View.Hide();
        }

        void OnJoinGame(JoinThroughDirectIPEvent evt)
        {
            ConnectionManager.StartClient(evt.ipAddress, evt.port);
        }

        void OnConnectionEvent(ConnectionEvent evt)
        {
            if (evt.status == ConnectStatus.Connecting)
            {
                View.Hide();
            }
        }
    }
}
