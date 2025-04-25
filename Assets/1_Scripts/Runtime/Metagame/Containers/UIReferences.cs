using UnityEngine.UIElements;
namespace RedGaint.Network.Runtime
{
    public class UIReferences
    {
        public Label NameLabel;
        public Button ProfileButton;
        public Button MultiplayerButton;
        public Button SinglePlayerButton;
        public Button QuitButton;

        public UIReferences(VisualElement root)
        {
            NameLabel = root.Q<Label>("titleLabel");
            ProfileButton = root.Q<Button>("ProfileButton");
            MultiplayerButton = root.Q<Button>("multiPlayerButton");
            SinglePlayerButton = root.Q<Button>("singlePlayerButton");
            QuitButton = root.Q<Button>("quitButton");
        }
    }
}