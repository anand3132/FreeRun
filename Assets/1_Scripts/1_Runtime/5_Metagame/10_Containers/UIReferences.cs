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
            NameLabel = root.Q<Label>("modelNameLabel");
            ProfileButton = root.Q<Button>("ProfileButton");
            MultiplayerButton = root.Q<Button>("MultiplayerButton");
            SinglePlayerButton = root.Q<Button>("SinglePlayerButton");
            QuitButton = root.Q<Button>("QuitButton");
        }
    }
}