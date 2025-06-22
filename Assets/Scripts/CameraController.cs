using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Probe targetProbe;//追従対象(Probe)
    private Vector3 _targetPosition;//対象の座標
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float distance = 5f;//カメラと対象の距離
        _targetPosition = targetProbe.transform.position;
        Vector3 direction = targetProbe.collisionTarget.transform.position - _targetPosition;//Probeから目標に向かうベクトル
        direction.Normalize();//単位ベクトル化
        transform.position = _targetPosition - direction * distance;//カメラの座標をProbeの後ろに配置
        transform.LookAt(_targetPosition);//向きを合わせる
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += targetProbe.transform.position - _targetPosition;
        _targetPosition = targetProbe.transform.position;
        //マウス移動量
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        //target中心に回転する
        transform.RotateAround(_targetPosition, Vector3.up, mouseX * Time.deltaTime * 200f);
        //カメラの垂直移動
        transform.RotateAround(_targetPosition, Vector3.right, mouseY * Time.deltaTime * 200f);
    }
}
