namespace RedGaint.Network.Runtime
{
    
    internal class LobbyStartedEvent : AppEvent
    {
        public float CountdownTime { get; }

        public LobbyStartedEvent(float countdownTime)
        {
            CountdownTime = countdownTime;
        }
    }

    internal class LobbyCountdownUpdateEvent : AppEvent
    {
        public int SecondsRemaining { get; }

        public LobbyCountdownUpdateEvent(int secondsRemaining)
        {
            SecondsRemaining = secondsRemaining;
        }
    }

    internal class LobbyGameStartingEvent : AppEvent { }

    internal class LobbyCancelledEvent : AppEvent { }
    internal class ResumeButtonClickedEvent : AppEvent { }

    internal class QuitButtonClickedEvent : AppEvent { }

    internal class MatchEndAcknowledgedEvent : AppEvent { }

    internal class StartMatchEvent : AppEvent { }

    internal class MenuToggleEvent : AppEvent { }

    internal class EndMatchEvent : AppEvent { }
}
