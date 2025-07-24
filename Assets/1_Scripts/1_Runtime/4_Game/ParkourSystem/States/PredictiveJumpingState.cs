using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles predictive jumping logic
    public class PredictiveJumpingState : PlayerState
    {
        private float jumpDuration = 0.8f; // Duration of the predictive jump
        private float jumpTimer = 0f;
        private Vector3[] trajectoryPoints;
        private int trajectoryLength;
        private bool useRootMotion = false;
        private int nextPoint = 0;

        public PredictiveJumpingState(PlayerStateMachine fsm, PlayerContext context, Vector3[] trajectory, bool rootMotion = false) : base(fsm, context)
        {
            trajectoryPoints = trajectory;
            trajectoryLength = trajectory != null ? trajectory.Length : 0;
            useRootMotion = rootMotion;
        }

        public override void Enter()
        {
            // Trigger predictive jump animation
            // Example: context.AnimatorWrapper.TriggerVault();
            jumpTimer = 0f;
            nextPoint = 0;
        }

        public override void Update()
        {
            jumpTimer += Time.deltaTime;
            float t = Mathf.Clamp01(jumpTimer / jumpDuration);

            if (!useRootMotion && trajectoryPoints != null && trajectoryLength > 1)
            {
                // Move along the trajectory points
                if (nextPoint < trajectoryLength)
                {
                    context.Transform.position = Vector3.Lerp(context.Transform.position, trajectoryPoints[nextPoint], 0.2f);
                    if (Vector3.Distance(context.Transform.position, trajectoryPoints[nextPoint]) < 0.1f)
                        nextPoint++;
                }
            }
            // If using root motion, let the animation drive the movement

            // Optionally, check for animation event or just use timer
            if (t >= 1.0f || nextPoint >= trajectoryLength)
            {
                // Decide next state based on context (e.g., ledge detected, hanging, or falling)
                // For now, transition to FallingState
                fsm.ChangeState(new FallingState(fsm, context));
            }
        }
    }
} 