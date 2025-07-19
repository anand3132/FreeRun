namespace RedGaint.ParkourSystem
{
    public abstract class PlayerState
    {
        protected PlayerStateMachine fsm;
        protected PlayerContext context;

        public PlayerState(PlayerStateMachine fsm, PlayerContext context)
        {
            this.fsm = fsm;
            this.context = context;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
    }

}