using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles balance walking on narrow beams
    public class BalanceWalkingState : PlayerState
    {
        public BalanceWalkingState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            // Set animator parameters for balance walk (if any)
            // Example: context.AnimatorWrapper.SetBalanceWalk(true);
        }

        public override void Update()
        {
            // Use EnvironmentScanner to check if still on beam
            bool isOnBeam = context.EnvironmentScanner != null && context.EnvironmentScanner.IsOnBeam(context.GroundCheck);
            context.CheckGrounded();
            if (!isOnBeam || !context.IsGrounded)
            {
                fsm.ChangeState(new FallingState(fsm, context));
                return;
            }

            // Camera-relative movement (usually restricted to forward/backward on beam)
            Vector2 moveInput = context.Input != null ? context.Input.MoveInput : Vector2.zero;
            Vector3 input = new Vector3(0f, 0f, moveInput.y); // Only allow forward/backward
            Vector3 moveDirection = context.Transform.TransformDirection(input.normalized);

            // Balance walk speed (slower than normal)
            float speed = context.MoveSpeed * 0.4f;
            Vector3 velocity = moveDirection * speed;
            velocity.y = context.Gravity * Time.deltaTime;
            context.Controller.Move(velocity * Time.deltaTime);

            // Update animator parameters
            if (context.AnimatorWrapper != null)
            {
                float moveAmount = Mathf.Abs(moveInput.y);
                context.AnimatorWrapper.SetMoveAmount(moveAmount);
                // Example: context.AnimatorWrapper.SetBalanceWalk(true);
            }
        }

        public override void Exit()
        {
            // Reset animator parameters for balance walk (if any)
            // Example: context.AnimatorWrapper.SetBalanceWalk(false);
        }
    }
} 