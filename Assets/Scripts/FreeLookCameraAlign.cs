using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class FreeLookCameraAlign : MonoBehaviour
{
    public CinemachineVirtualCameraBase freeLookCamera; // 仮想カメラ
    public Probe targetProbe; // 追従対象のProbe
    public float distance; // カメラとProbeの距離
    private Transform _target;
    void Start()
    {
        _target = targetProbe.collisionTarget.transform;

        Vector3 offset = -_target.forward * distance + Vector3.up * distance;
        freeLookCamera.transform.position = _target.position + offset;
        freeLookCamera.transform.LookAt(_target.position);
    }
    
}
