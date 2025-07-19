using UnityEngine;

namespace RedGaint.ParkourSystem
{
    public class AnimatorControllerWrapper
    {
        private readonly Animator animator;

        public AnimatorControllerWrapper(Animator animator)
        {
            this.animator = animator;
        }

        public void SetGrounded(bool value) => animator.SetBool(AnimationParams.IsGrounded, value);
        public void SetFalling(bool value) => animator.SetBool(AnimationParams.IsFalling, value);
        public void TriggerJump() => animator.SetTrigger(AnimationParams.Jump);
        public void TriggerDoubleJump() => animator.SetTrigger(AnimationParams.DoubleJump);
        public void TriggerLand() => animator.SetTrigger(AnimationParams.Land);
        public void TriggerFire() => animator.SetTrigger(AnimationParams.Fire);
    }
}