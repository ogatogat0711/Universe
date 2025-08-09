using UnityEngine;

namespace ParallelCascades.Common.Runtime
{
    [ExecuteInEditMode]
    public class DisableRotation : MonoBehaviour
    {
        private void Update()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}