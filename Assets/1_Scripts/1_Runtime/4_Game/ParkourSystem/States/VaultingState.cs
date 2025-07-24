using UnityEngine;

namespace RedGaint.Games.ParkourSystem.States
{
    // Handles vaulting over obstacles
    public class VaultingState : PlayerState
    {
        private float vaultDuration = 0.6f; // Duration of the vault
        private float vaultTimer = 0f;
        private Vector3 startPos;
        private Vector3 endPos;
        private bool useRootMotion = false;

        public VaultingState(PlayerStateMachine fsm, PlayerContext context, Vector3 vaultEndPos, bool rootMotion = false) : base(fsm, context)
        {
            endPos = vaultEndPos;
            useRootMotion = rootMotion;
        }

        public override void Enter()
        {
            // Trigger vault animation
            // Example: context.AnimatorWrapper.TriggerVault();
            startPos = context.Transform.position;
            vaultTimer = 0f;
        }

        public override void Update()
        {
            vaultTimer += Time.deltaTime;
            float t = Mathf.Clamp01(vaultTimer / vaultDuration);

            if (!useRootMotion)
            {
                // Manually interpolate position over the obstacle
                context.Transform.position = Vector3.Lerp(startPos, endPos, t);
            }
            // If using root motion, let the animation drive the movement

            // Optionally, check for animation event or just use timer
            if (t >= 1.0f)
            {
                fsm.ChangeState(new GroundedState(fsm, context));
            }
        }
    }
} 