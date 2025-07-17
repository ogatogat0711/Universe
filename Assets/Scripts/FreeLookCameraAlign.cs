using Unity.Cinemachine;
using UnityEngine;

public class FreeLookCameraAlign : MonoBehaviour
{
    public CinemachineVirtualCameraBase freeLookCamera; // 仮想カメラ
    public Probe targetProbe; // 追従対象のProbe
    private float _distance; // カメラとProbeの距離
    private Transform _target;
    void Start()
    {
        _target = targetProbe.collisionTarget.transform;
        CinemachineThirdPersonFollow follow = freeLookCamera.GetComponent<CinemachineThirdPersonFollow>();
        _distance = follow.CameraDistance;
    }

    void LateUpdate()
    {
        if(!freeLookCamera.IsLive) return; // カメラがアクティブでない場合は何もしない
        
        Vector3 direction = targetProbe.transform.position - _target.position; // Probeから目標に向かうベクトル
        direction.Normalize();
        Vector3 cameraPos = _target.position + direction * _distance;
        
        freeLookCamera.transform.position = cameraPos; // カメラの位置を更新
        freeLookCamera.transform.LookAt(targetProbe.transform);
    }
    
}
