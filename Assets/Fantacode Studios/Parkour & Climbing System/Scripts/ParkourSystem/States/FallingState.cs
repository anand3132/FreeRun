using UnityEngine;

namespace RedGaint.ParkourSystem
{
    public class FallingState : PlayerState
    {
        private Vector3 velocity;

        public FallingState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context)
        {
        }

        public override void Enter()
        {
            context.AnimatorWrapper.SetFalling(true);
            context.CoyoteTimer = 0f;

            // Reset velocity or inherit previous if needed
            velocity = context.VerticalVelocity;
        }

        public override void Exit()
        {
            context.AnimatorWrapper.SetFalling(false);
        }

        public override void Update()
        {
            context.CoyoteTimer -= Time.deltaTime;

            if (context.IsGrounded)
            {
                fsm.ChangeState(new LandingState(fsm, context));
                return;
            }

            // Handle double jump during fall
            if (context.Input.JumpPressed && context.CanDoubleJump)
            {
                fsm.ChangeState(new DoubleJumpingState(fsm, context));
                return;
            }

            ApplyFallGravity();
            ApplyAirControl();

            // Apply movement
            context.Controller.Move(velocity * Time.deltaTime);
        }

        private void ApplyFallGravity()
        {
            velocity.y += context.Gravity * 1.5f * Time.deltaTime;
            context.VerticalVelocity = velocity;
        }

        private void ApplyAirControl()
        {
            Vector3 inputDir = new Vector3(context.Input.MoveInput.x, 0, context.Input.MoveInput.y);
            Vector3 moveDir = context.Transform.TransformDirection(inputDir.normalized);
            velocity.x = moveDir.x * context.MoveSpeed;
            velocity.z = moveDir.z * context.MoveSpeed;
        }
    }
}