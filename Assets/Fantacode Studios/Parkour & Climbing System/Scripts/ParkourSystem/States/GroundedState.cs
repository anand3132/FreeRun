using UnityEngine;

namespace RedGaint.ParkourSystem
{
    public class GroundedState : PlayerState
    {
        public GroundedState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            context.CanDoubleJump = true;
            context.CoyoteTimer = 0.2f; // Small grace period after landing
            context.AnimatorWrapper.SetGrounded(true);
        }

        public override void Exit()
        {
            context.AnimatorWrapper.SetGrounded(false);
        }

        public override void Update()
        {
            context.CheckGrounded();

            if (!context.IsGrounded)
            {
                fsm.ChangeState(new FallingState(fsm, context));
                return;
            }

            if (context.Input.JumpPressed)
            {
                fsm.ChangeState(new JumpingState(fsm, context));
                return;
            }

            if (context.Input.FirePressed)
            {
                Fire();
            }

            MovePlayer();
        }

        private void Fire()
        {
            context.AnimatorWrapper.TriggerFire();
            // TODO: Handle actual fire logic (e.g. shooting a projectile)
        }

        private void MovePlayer()
        {
            Vector3 input = new Vector3(context.Input.MoveInput.x, 0f, context.Input.MoveInput.y);
            Vector3 moveDirection = context.Transform.TransformDirection(input.normalized) * context.MoveSpeed;

            // Apply slight downward force to stick to ground
            moveDirection.y = context.Gravity * Time.deltaTime;

            context.Controller.Move(moveDirection * Time.deltaTime);
        }
    }
}