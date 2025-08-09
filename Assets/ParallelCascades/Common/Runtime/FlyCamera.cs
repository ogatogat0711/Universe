using UnityEngine;

namespace ParallelCascades.Common.Runtime
{
    public class FlyCamera : MonoBehaviour
    {
        public float MaxVAngle = 89;
        public float MinVAngle = -89;
    
        public float FlyRotationSpeed = 1.5f;
        public float FlyRotationSharpness = 999999;
        public float FlyMoveSharpness = 10;
        public float FlyMaxMoveSpeed = 35;
        public float FlySprintSpeedBoost = 5;

        private Vector3 m_CurrentMoveVelocity;
        private float m_PitchAngle;
        private Vector3 m_PlanarForward = Vector3.forward;

        private bool m_IgnoreInput;
        
        private struct CameraInputs
        {
            public Vector3 Move;
            public Vector2 Look;
            public bool Sprint;
        }
        
        void Update()
        {
            CameraInputs cameraInputs = new CameraInputs
            {
                Move = new Vector3(
                    (Input.GetKey(KeyCode.D) ? 1f : 0f) + (Input.GetKey(KeyCode.A) ? -1f : 0f),
                    (Input.GetKey(KeyCode.E) ? 1f : 0f) + (Input.GetKey(KeyCode.Q) ? -1f : 0f),
                    (Input.GetKey(KeyCode.W) ? 1f : 0f) + (Input.GetKey(KeyCode.S) ? -1f : 0f)),
                Look = new Vector2(
                    Input.GetAxis("Mouse X"),
                    Input.GetAxis("Mouse Y")),
                Sprint = Input.GetKey(KeyCode.LeftShift),
            };
            cameraInputs.Move = cameraInputs.Move *
                                Mathf.Clamp(Vector3.Magnitude(cameraInputs.Move),0,1); // Clamp move inputs magnitude

            if (m_IgnoreInput)
            { 
                return;
            }
        
            // Yaw
            float yawAngleChange = cameraInputs.Look.x * FlyRotationSpeed;
            Quaternion yawRotation = Quaternion.Euler(Vector3.up *  yawAngleChange);

            m_PlanarForward = yawRotation * m_PlanarForward;

            // Pitch
            m_PitchAngle += -cameraInputs.Look.y * FlyRotationSpeed;
            m_PitchAngle = Mathf.Clamp(m_PitchAngle, MinVAngle,
                MaxVAngle);
            Quaternion pitchRotation = Quaternion.Euler(Vector3.right *  m_PitchAngle);

            // Final rotation
            Quaternion targetRotation =
                Quaternion.LookRotation(m_PlanarForward, Vector3.up) * pitchRotation;
        
            float deltaTime = Time.deltaTime;
        
            var cameraTransform = UnityEngine.Camera.main.transform;
        
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation,
                GetSharpnessInterpolant(FlyRotationSharpness, deltaTime));

            // Move
            Vector3 worldMoveInputs = cameraTransform.rotation * cameraInputs.Move;
            float finalMaxSpeed = FlyMaxMoveSpeed;
            if (cameraInputs.Sprint)
            {
                finalMaxSpeed *= FlySprintSpeedBoost;
            }

            m_CurrentMoveVelocity = Vector3.Lerp(m_CurrentMoveVelocity,
                worldMoveInputs * finalMaxSpeed,
                GetSharpnessInterpolant(FlyMoveSharpness, deltaTime));
            cameraTransform.position += m_CurrentMoveVelocity * deltaTime;
        }
        
        private static float GetSharpnessInterpolant(float sharpness, float dt)
        {
            return Mathf.Clamp(1f - Mathf.Exp(-sharpness * dt),0,1);
        }
    }
}