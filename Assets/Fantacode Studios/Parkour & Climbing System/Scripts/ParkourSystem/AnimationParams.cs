using UnityEngine;

namespace RedGaint.ParkourSystem
{
    public static class AnimationParams
    {
        // Bools
        public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        public static readonly int IsFalling = Animator.StringToHash("IsFalling");

        // Triggers
        public static readonly int Jump = Animator.StringToHash("Jump");
        public static readonly int DoubleJump = Animator.StringToHash("DoubleJump");
        public static readonly int Land = Animator.StringToHash("Land");
        public static readonly int Fire = Animator.StringToHash("Fire");

        // Add more as needed
    }
}