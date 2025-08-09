using UnityEngine;

namespace ParallelCascades.Common.Runtime
{
    public class RotatingBody : MonoBehaviour
    {
        [Tooltip("The value is in degrees per second - x, y and z Euler rotation.")]
        [SerializeField] private Vector3 m_RotationSpeedPerAxis;

        private void Update()
        {
            if (m_RotationSpeedPerAxis != Vector3.zero)
            {
                Quaternion rotation = Quaternion.Euler(m_RotationSpeedPerAxis * Time.deltaTime);
                
                transform.localRotation *= rotation;
            }
        }
    }
}