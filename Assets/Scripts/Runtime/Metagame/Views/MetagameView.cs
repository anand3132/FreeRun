using UnityEngine;

namespace RedGaint.Network.Runtime
{
    /// <summary>
    /// Main view of the <see cref="MetagameApplication"></see>
    /// </summary>
    public class MetagameView : View<MetagameApplication>
    {
        internal MainMenuView MainMenu => m_MainMenuView;
        [SerializeField] private MainMenuView m_MainMenuView;

        internal MatchmakerView Matchmaker => m_MatchmakerView;
        [SerializeField] private MatchmakerView m_MatchmakerView;
        
        internal DirectIPView DirectIP => m_DirectIPView;
        [SerializeField] private DirectIPView m_DirectIPView;
        
        internal ClientConnectingView ClientConnecting => m_ClientConnectingView;
        [SerializeField] private ClientConnectingView m_ClientConnectingView;

        internal ModelSelectionView ModelSelectionView=>m_ModelSelectionView;
        [SerializeField] ModelSelectionView m_ModelSelectionView;
        internal LoginView LoginView => m_LoginView;
        [SerializeField] private LoginView m_LoginView;
    }
}
