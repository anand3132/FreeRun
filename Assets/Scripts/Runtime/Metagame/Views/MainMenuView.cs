using UnityEngine;
using UnityEngine.UIElements;
using System;
namespace RedGaint.Network.Runtime
{

    internal class MainMenuView : View<MetagameApplication>
    {
        MainMenuModeContext modeContext;
        bool IsFestivalSeason()
        {
            var today = DateTime.UtcNow;
            return today.Month == 12 && today.Day >= 20; // Dec 20 to Dec 31 for example
        }

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var uiRefs = new UIReferences(root);

            modeContext = new MainMenuModeContext(uiRefs);

            if (IsFestivalSeason()&&IsUserLoggedIn())
                modeContext.SetMode(MenuMode.Festival);
            else if (IsUserLoggedIn())
                modeContext.SetMode(MenuMode.User);
            else
                modeContext.SetMode(MenuMode.Guest);
        }

        private void OnDisable()
        {
          modeContext.ClearContext();
        }

        bool IsUserLoggedIn() => !string.IsNullOrEmpty(GameProfileManager.Current?.Username);
    }
}


