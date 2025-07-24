using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles wall running logic
    public class WallRunningState : PlayerState
    {
        private float wallRunDuration = 1.0f; // Duration of the wall run
        private float wallRunTimer = 0f;
        private Vector3 wallDirection;
        private Vector3 wallNormal;

        public WallRunningState(PlayerStateMachine fsm, PlayerContext context, Vector3 direction, Vector3 normal) : base(fsm, context)
        {
            wallDirection = direction;
            wallNormal = normal;
        }

        public override void Enter()
        {
            // Trigger wall run animation
            // Example: context.AnimatorWrapper.TriggerWallRun();
            wallRunTimer = 0f;
        }

        public override void Update()
        {
            wallRunTimer += Time.deltaTime;
            float t = Mathf.Clamp01(wallRunTimer / wallRunDuration);

            // Move the character along the wall
            float speed = context.MoveSpeed * 1.2f; // Wall run speed (can be tuned)
            Vector3 move = wallDirection.normalized * speed * Time.deltaTime;
            context.Controller.Move(move);

            // Optionally, apply gravity away from the wall
            context.VerticalVelocity.y += context.Gravity * 0.5f * Time.deltaTime;
            context.Controller.Move(Vector3.up * context.VerticalVelocity.y * Time.deltaTime);

            // Optionally, check for input to jump off the wall
            if (context.Input != null && context.Input.JumpPressed)
            {
                // Jump off the wall (add force away from wallNormal)
                context.VerticalVelocity = wallNormal * context.JumpForce + Vector3.up * context.JumpForce;
                fsm.ChangeState(new JumpingState(fsm, context));
                return;
            }

            // End wall run if timer expires or player loses contact
            if (t >= 1.0f /* or !IsStillOnWall() */)
            {
                fsm.ChangeState(new FallingState(fsm, context));
            }
        }
    }
} 