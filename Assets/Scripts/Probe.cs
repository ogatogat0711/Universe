using UnityEngine;
using UnityEngine.Serialization;

public class Probe : MonoBehaviour
{
    public float speed = 15f;
    private Rigidbody _rigidbody;
    private Vector3 _velocity;
    private float _horizontal;
    private float _vertical;
    public Camera followingCamera;
    public GameObject collisionTarget;
    public bool canMove;//移動可能かどうかのフラグ. UIボタンで制御
    public int fuel; // 燃料
    public int maxFuel = 100;// 最大燃料
    public int fuelConsumptionRatioOfManipulation = 3; // 操作時の燃料消費率
    public int fuelConsumptionRatioOfAutoMove = 1; // 自動移動時の燃料消費率
    private int _fuelConsumption; // 燃料消費量
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fuel = maxFuel;
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            _horizontal = Input.GetAxis("Horizontal");
            _vertical = Input.GetAxis("Vertical");
            _fuelConsumption = Mathf.RoundToInt(Mathf.Abs(_horizontal) + Mathf.Abs(_vertical));
            _fuelConsumption *= fuelConsumptionRatioOfManipulation; // 燃料消費量を計算
        }
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            Vector3 cameraDirection= Vector3.Scale(followingCamera.transform.forward, new Vector3(1, 0, 1)).normalized;//カメラの見ている方向からXZ平面の単位ベクトルを取得
            Vector3 moveDirection = cameraDirection * _vertical + followingCamera.transform.right * _horizontal;//キー入力から移動方向を決定
            _rigidbody.linearVelocity = moveDirection * speed;
            fuel -= _fuelConsumption;
            Debug.Log(fuel);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == collisionTarget)
        {
            Debug.Log("Hit");
            Time.timeScale = 0f;
        }
    }
}
