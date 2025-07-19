using UnityEngine;

namespace RedGaint.ParkourSystem
{
    public class JumpingState : PlayerState
    {
        private Vector3 velocity;

        public JumpingState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            // Apply initial upward force
            velocity = new Vector3(0f, context.JumpForce, 0f);

            // Trigger animation
            context.AnimatorWrapper.TriggerJump();

            Debug.Log("JumpingState Entered");
        }

        public override void Update()
        {
            Debug.Log("JumpingState Update");

            // Allow double jump while in the air
            if (context.Input.JumpPressed && context.CanDoubleJump)
            {
                fsm.ChangeState(new DoubleJumpingState(fsm, context));
                return;
            }

            // Apply gravity
            velocity.y += context.Gravity * Time.deltaTime;

            // Apply air control (horizontal movement)
            ApplyAirControl();

            // Move player
            context.Controller.Move(velocity * Time.deltaTime);

            // If moving downward, transition to falling
            if (velocity.y <= 0)
            {
                fsm.ChangeState(new FallingState(fsm, context));
            }
        }

        private void ApplyAirControl()
        {
            Vector3 input = new Vector3(context.Input.MoveInput.x, 0, context.Input.MoveInput.y);
            Vector3 move = context.Transform.TransformDirection(input.normalized) * context.MoveSpeed;
            velocity.x = move.x;
            velocity.z = move.z;
        }
    }
}