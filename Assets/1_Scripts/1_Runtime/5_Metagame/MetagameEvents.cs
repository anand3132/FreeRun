namespace RedGaint.Network.Runtime
{
    
    internal class EnterLobbyQueueEvent : AppEvent { }
    internal class EnterMainMenuEvent : AppEvent { }

    internal class EnterUserProfileEvent : AppEvent { }
    internal class EnterLoginEvent : AppEvent { }
    
    internal class ExitMatchmakerQueueEvent : AppEvent { }
    
    
    internal class EnterModelSelectionEvent : AppEvent { }
    internal class CancelConnectionEvent: AppEvent { }
    
    /// <summary>
    /// Called when a match is entered (I.E: after matchmaking finds enough players)
    /// </summary>
    internal class MatchEnteredEvent : AppEvent { }
    internal class StartSingleplayer : AppEvent { }
    
    internal class PlayerSignedIn : AppEvent
    {
        public UnityServicesInitializer.SignInMethod  SignInMethod { get; private set; }
        public bool Success { get; private set; }
        public string PlayerId { get; private set; }

        public PlayerSignedIn(bool success, string playerId, UnityServicesInitializer.SignInMethod signInMethod)
        {
            SignInMethod = signInMethod;
            Success = success;
            PlayerId = playerId;
        }
    }
}
