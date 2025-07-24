using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles jump logic and transitions to Falling or DoubleJumping
    public class JumpingState : PlayerState
    {
        public JumpingState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            // Apply initial upward force for jump
            context.VerticalVelocity.y = Mathf.Sqrt(2 * Mathf.Abs(context.Gravity) * context.JumpForce);

            // Set grounded and falling parameters for jumping
            if (context.AnimatorWrapper != null)
            {
                context.AnimatorWrapper.SetGrounded(false);
                context.AnimatorWrapper.SetFalling(false);
            }
        }

        public override void Update()
        {
            // Apply gravity
            context.VerticalVelocity.y += context.Gravity * Time.deltaTime;

            // Air control (horizontal movement)
            Vector2 moveInput = context.Input != null ? context.Input.MoveInput : Vector2.zero;
            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 moveDirection = context.Transform.TransformDirection(input.normalized);
            float speed = context.MoveSpeed;
            Vector3 velocity = moveDirection * speed;
            velocity.y = context.VerticalVelocity.y;
            context.Controller.Move(velocity * Time.deltaTime);

            // Transition to FallingState if moving downward
            if (context.VerticalVelocity.y <= 0)
            {
                fsm.ChangeState(new FallingState(fsm, context));
            }
        }
    }
} 