using UnityEngine.UIElements;
namespace RedGaint.Network.Runtime
{
    internal class MainMenuView : View<MetagameApplication>
    {
        Button m_MultiplayerButton;
        Button m_SinglePlayerButton;
        Button m_QuitButton;
        Button m_ModelSelectionButton;

        Label m_TitleLabel;
        UIDocument m_UIDocument;

        void Awake()
        {
            m_UIDocument = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            var root = m_UIDocument.rootVisualElement;
                
            m_ModelSelectionButton = root.Q<Button>("modelSelectionButton");
            m_ModelSelectionButton.RegisterCallback<ClickEvent>(OnClickModelSelect);
            
            m_SinglePlayerButton = root.Q<Button>("singlePlayerButton");
            m_SinglePlayerButton.RegisterCallback<ClickEvent>(OnClickSinglePlay);
            
            m_MultiplayerButton = root.Q<Button>("multiPlayerButton");
            m_MultiplayerButton.RegisterCallback<ClickEvent>(OnClickMultiplay);

            m_QuitButton = root.Q<Button>("quitButton");
            m_QuitButton.RegisterCallback<ClickEvent>(OnClickQuit);
        

            m_TitleLabel = root.Query<Label>("titleLabel");
            m_TitleLabel.text = "Free Run";
        }

        void OnDisable()
        {
            m_SinglePlayerButton.UnregisterCallback<ClickEvent>(OnClickModelSelect);
            m_MultiplayerButton.UnregisterCallback<ClickEvent>(OnClickMultiplay);
            m_SinglePlayerButton.UnregisterCallback<ClickEvent>(OnClickSinglePlay);
            m_QuitButton.UnregisterCallback<ClickEvent>(OnClickQuit);
        }

        void OnClickMultiplay(ClickEvent evt)
        {
            Broadcast(new EnterLobbyQueueEvent());
        }

        void OnClickSinglePlay(ClickEvent evt)
        {
            Broadcast(new EnterMatchmakerQueueEvent("default"));
            
        }
        void OnClickModelSelect(ClickEvent evt)
        {
            Broadcast(new EnterModelSelectionEvent());
        }

        void OnClickQuit(ClickEvent evt)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
