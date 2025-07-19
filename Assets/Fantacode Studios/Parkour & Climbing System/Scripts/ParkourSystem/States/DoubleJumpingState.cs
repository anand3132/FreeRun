using UnityEngine;

namespace RedGaint.ParkourSystem
{
    public class DoubleJumpingState : PlayerState
    {
        public DoubleJumpingState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            context.CanDoubleJump = false;

            // Set vertical velocity for double jump
            context.VerticalVelocity.y = 7f;

            // Use animator wrapper
            context.AnimatorWrapper.TriggerDoubleJump();
        }

        public override void Update()
        {
            ApplyAirControl();

            // Apply gravity
            context.VerticalVelocity.y += Physics.gravity.y * 1.5f * Time.deltaTime;

            // Combine movement
            Vector3 movement = context.MoveInputDirection * 3f; // horizontal
            movement += context.VerticalVelocity;               // vertical
            context.Controller.Move(movement * Time.deltaTime);

            // Switch to falling
            if (context.VerticalVelocity.y <= 0)
            {
                fsm.ChangeState(new FallingState(fsm, context));
            }
        }

        private void ApplyAirControl()
        {
            var moveInput = context.Input.MoveInput;
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            camForward.y = 0;
            camRight.y = 0;

            camForward.Normalize();
            camRight.Normalize();

            context.MoveInputDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        }
    }
}