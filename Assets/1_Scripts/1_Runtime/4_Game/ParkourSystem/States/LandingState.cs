using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles landing logic after falling or jumping
    public class LandingState : PlayerState
    {
        private float landingTimer = 0f;
        private float landingDuration = 0.2f; // Adjust as needed for animation length

        public LandingState(PlayerStateMachine fsm, PlayerContext context) : base(fsm, context) { }

        public override void Enter()
        {
            // Set grounded and falling parameters for landing
            if (context.AnimatorWrapper != null)
            {
                context.AnimatorWrapper.SetGrounded(true);
                context.AnimatorWrapper.SetFalling(false);
            }
            // Reset jump/fall state
            context.VerticalVelocity = Vector3.zero;
            landingTimer = 0f;
        }

        public override void Update()
        {
            // Wait for landing animation to finish (or use animation event in production)
            landingTimer += Time.deltaTime;
            if (landingTimer >= landingDuration)
            {
                fsm.ChangeState(new GroundedState(fsm, context));
            }
        }
    }
} 