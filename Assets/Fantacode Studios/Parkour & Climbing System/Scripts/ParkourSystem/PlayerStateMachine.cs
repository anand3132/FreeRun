using UnityEngine;

namespace RedGaint.ParkourSystem
{
    public class PlayerStateMachine : MonoBehaviour
    {
        private PlayerState currentState;
        public PlayerContext context { get; private set; }

        private void Awake()
        {
            context = new PlayerContext
            {
                Transform = transform,
                Animator = GetComponent<Animator>(),
                Controller = GetComponent<CharacterController>(),
                Input = GetComponent<PlayerInputReader>()
            };

            context.Init(this);
        }

        private void Start()
        {
            if (context.Animator == null) context.Animator = GetComponent<Animator>();
            if (context.Controller == null) context.Controller = GetComponent<CharacterController>();
            if (context.Input == null) context.Input = GetComponent<PlayerInputReader>();

            // Ensure wrapper is initialized after animator is assigned
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