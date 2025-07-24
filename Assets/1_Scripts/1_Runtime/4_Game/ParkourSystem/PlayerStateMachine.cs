using RedGaint.Games.ParkourSystem.States;
using UnityEngine;

namespace RedGaint.Games.ParkourSystem
{
    public class PlayerStateMachine : MonoBehaviour
    {
        [SerializeField]
        public PlayerContext context;

        private PlayerState currentState;
        public PlayerContext Context => context;

        private void Awake()
        {
            if (context == null)
                context = new PlayerContext();
            context.Transform = transform;
            context.Animator = GetComponent<Animator>();
            context.Controller = GetComponent<CharacterController>();
            context.Input = GetComponent<PlayerInputReader>();
            context.Init(this);
        }

        private void Start()
        {
            if (context.Animator == null) context.Animator = GetComponent<Animator>();
            if (context.Controller == null) context.Controller = GetComponent<CharacterController>();
            if (context.Input == null) context.Input = GetComponent<PlayerInputReader>();
            context.Init(this);
            ChangeState(new GroundedState(this, context));
        }

        private void Update()
        {
            context.CheckGrounded();
            currentState?.Update();
        }

        private void FixedUpdate()
        {
            currentState?.FixedUpdate();
        }

        public void ChangeState(PlayerState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }
    }
} 