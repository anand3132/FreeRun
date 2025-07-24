using UnityEngine;

namespace RedGaint.Games.ParkourSystem
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        public Transform followTarget;
        public float distance = 5f;
        public float height = 2f;
        public float followSpeed = 8f;
        public float rotationSpeed = 120f;

        [Header("Auto-Follow Settings")]
        public bool autoFollowPlayer = true;
        public float followPlayerSpeed = 5f;

        [Header("Turn Back Cinematic Settings")]
        public float turnBackDuration = 1.0f;
        public float turnBackHeight = 4f;
        public float turnBackDistance = 8f;

        [Header("Cinematic Slide Settings")]
        public float holdDuration = 0.5f;
        public float slideDistance = 3f;
        public float slideSpeed = 5f;
        public float orbitSpeed = 120f;

        private float yaw;
        private float pitch;
        public float minPitch = -30f;
        public float maxPitch = 60f;

        private bool isTurningBack = false;
        private float turnBackTimer = 0f;
        private float originalHeight;
        private float originalDistance;

        private enum CameraState { Follow, Hold, Slide, Orbit }
        private CameraState cameraState = CameraState.Follow;
        private float holdTimer = 0f;
        private Vector3 slideDirection;
        private Vector3 slideStartPos;
        private float orbitTargetYaw;
        private bool orbitingLeft = true;

        void LateUpdate()
        {
            if (followTarget == null) return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Cinematic turn-back (legacy, can be combined with new state machine if needed)
            if (isTurningBack)
            {
                turnBackTimer += Time.deltaTime;
                float t = Mathf.Clamp01(turnBackTimer / turnBackDuration);
                float currentHeight = Mathf.Lerp(originalHeight, turnBackHeight, t);
                float currentDistance = Mathf.Lerp(originalDistance, turnBackDistance, t);
                float targetYaw = followTarget.eulerAngles.y;
                yaw = Mathf.LerpAngle(yaw, targetYaw, t);
                Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
                Vector3 targetPos = followTarget.position + Vector3.up * currentHeight;
                Vector3 desiredPos = targetPos - rotation * Vector3.forward * currentDistance;
                transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
                transform.LookAt(targetPos);
                if (t >= 1.0f)
                {
                    isTurningBack = false;
                    height = turnBackHeight;
                    distance = turnBackDistance;
                }
                return;
            }

            // Cinematic camera state machine
            switch (cameraState)
            {
                case CameraState.Follow:
                    HandleFollow(mouseX, mouseY);
                    break;
                case CameraState.Hold:
                    holdTimer += Time.deltaTime;
                    // Camera holds position, still looks at player
                    transform.LookAt(followTarget.position + Vector3.up * height);
                    if (holdTimer > holdDuration)
                    {
                        // Choose left or right for slide based on camera/player relative position
                        Vector3 toPlayer = (followTarget.position - transform.position).normalized;
                        Vector3 camRight = Vector3.Cross(Vector3.up, toPlayer).normalized;
                        orbitingLeft = Random.value > 0.5f;
                        slideDirection = orbitingLeft ? -camRight : camRight;
                        slideStartPos = transform.position;
                        cameraState = CameraState.Slide;
                    }
                    break;
                case CameraState.Slide:
                    // Move camera sideways
                    transform.position += slideDirection * slideSpeed * Time.deltaTime;
                    transform.LookAt(followTarget.position + Vector3.up * height);
                    if (Vector3.Distance(transform.position, slideStartPos) > slideDistance)
                    {
                        // Set up orbit
                        orbitTargetYaw = followTarget.eulerAngles.y;
                        cameraState = CameraState.Orbit;
                    }
                    break;
                case CameraState.Orbit:
                    // Smoothly orbit behind the player
                    float targetYaw = followTarget.eulerAngles.y;
                    float orbitStep = orbitSpeed * Time.deltaTime;
                    yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, orbitStep);
                    Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0);
                    Vector3 orbitTargetPos = followTarget.position + Vector3.up * height;
                    Vector3 orbitDesiredPos = orbitTargetPos - orbitRotation * Vector3.forward * distance;
                    transform.position = Vector3.Lerp(transform.position, orbitDesiredPos, followSpeed * Time.deltaTime);
                    transform.LookAt(orbitTargetPos);
                    if (Mathf.Abs(Mathf.DeltaAngle(yaw, targetYaw)) < 2f)
                    {
                        cameraState = CameraState.Follow;
                    }
                    break;
            }
        }

        private void HandleFollow(float mouseX, float mouseY)
        {
            if (autoFollowPlayer && Mathf.Abs(mouseX) < 0.01f)
            {
                float targetYaw = followTarget.eulerAngles.y;
                yaw = Mathf.LerpAngle(yaw, targetYaw, followPlayerSpeed * Time.deltaTime);
            }
            else
            {
                yaw += mouseX * rotationSpeed * Time.deltaTime;
            }
            pitch -= mouseY * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            Quaternion normalRotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 normalTargetPos = followTarget.position + Vector3.up * height;
            Vector3 normalDesiredPos = normalTargetPos - normalRotation * Vector3.forward * distance;
            transform.position = Vector3.Lerp(transform.position, normalDesiredPos, followSpeed * Time.deltaTime);
            transform.LookAt(normalTargetPos);
        }

        // Call this from movement code when a cinematic turn-back is detected
        public void StartTurnBackCinematic()
        {
            isTurningBack = true;
            turnBackTimer = 0f;
            originalHeight = height;
            originalDistance = distance;
        }

        // Call this from movement code when the player moves toward the camera
        public void StartCinematicSlide()
        {
            if (cameraState == CameraState.Follow)
            {
                cameraState = CameraState.Hold;
                holdTimer = 0f;
            }
        }
    }
} 