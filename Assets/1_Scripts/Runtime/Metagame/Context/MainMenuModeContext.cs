using System;
namespace RedGaint.Network.Runtime
{
    public enum MenuMode
    {
        Guest,
        User,
        Festival 
    }
    public class MainMenuModeContext
    {
        private readonly UIReferences _ui;
        private IMenuModeHandler _handler;
        private IMenuModeHandler _sharedHandler;
        public MainMenuModeContext(UIReferences ui)
        {
            _ui = ui;
        }

        public void SetMode(MenuMode mode)
        {
            _handler?.Cleanup();

            _handler = mode switch
            {
                MenuMode.Guest => new GuestMenuHandler(_ui),
                MenuMode.User => new UserMenuHandler(_ui),
                MenuMode.Festival => new FestivalMenuHandler(_ui),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            _handler.Initialize();
            _sharedHandler = new SharedUIHandler(_ui);
            _sharedHandler.Initialize();
        }

        public void ClearContext()
        {
            _handler.Cleanup();
            _sharedHandler.Cleanup();
        }

    }
}