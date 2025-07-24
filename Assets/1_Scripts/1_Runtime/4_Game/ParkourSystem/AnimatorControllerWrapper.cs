using UnityEngine;

namespace RedGaint.Games.ParkourSystem
{
    public class AnimatorControllerWrapper
    {
        private Animator animator;

        public AnimatorControllerWrapper(Animator animator)
        {
            this.animator = animator;
        }

        // Grounded/Movement
        public void SetGrounded(bool isGrounded) => animator.SetBool("IsGrounded", isGrounded);
        public void SetFalling(bool isFalling) => animator.SetBool("isFalling", isFalling);
        public void SetMoveAmount(float moveAmount) => animator.SetFloat("moveAmount", moveAmount);
        public void SetIdleType(float idleType) => animator.SetFloat("idleType", idleType);
        public void SetCrouchType(float crouchType) => animator.SetFloat("crouchType", crouchType);
        public void SetRotation(float rotation) => animator.SetFloat("rotation", rotation);

        // Balance walking
        public void SetBalanceWalk(bool isBalancing) => animator.SetBool("isBalanceWalking", isBalancing);

        // Turning/Quick turn
        public void TriggerTurn() => animator.SetTrigger("Turn");

        // Vaulting
        public void TriggerVault() => animator.SetTrigger("Vault");

        // Climbing
        public void TriggerClimb() => animator.SetTrigger("Climb");

        // Hanging
        public void TriggerHang() => animator.SetTrigger("Hang");

        // Wall running
        public void TriggerWallRun() => animator.SetTrigger("WallRun");

        // Ledge grabbing
        public void TriggerLedgeGrab() => animator.SetTrigger("LedgeGrab");
        // Add more methods as needed for your animator parameters/triggers
    }
} 