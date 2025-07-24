using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles falling logic and transitions to Landing or Grounded
    public class FallingState : PlayerState
    {
        public FallingState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            // Set animator parameters for falling
            if (context.AnimatorWrapper != null)
            {
                context.AnimatorWrapper.SetFalling(true);
                context.AnimatorWrapper.SetGrounded(false);
            }
            context.CoyoteTimer = 0f;
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

            // Check for landing
            context.CheckGrounded();
            if (context.IsGrounded)
            {
                fsm.ChangeState(new LandingState(fsm, context));
                return;
            }
        }
    }
} 