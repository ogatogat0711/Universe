using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class Probe : MonoBehaviour
{
    public float speed = 15f;
    private Rigidbody _rigidbody;
    private Vector3 _velocity;
    private float _horizontal;
    private float _vertical;
    private float _rotationSpeed = 25f;
    //public Camera followingCamera;
    public CinemachineVirtualCameraBase followingVirtualCamera;
    public GameObject collisionTarget;
    public bool canMove;//移動可能かどうかのフラグ
    public bool isManipulating; // 操作中かどうかのフラグ
    private bool _notNeedToRotate;//回転が必要かのフラグ
    public int fuel; // 燃料
    public int maxFuel = 100;// 最大燃料
    public int fuelConsumptionRatioOfManipulation = 3; // 操作時の燃料消費率
    public int fuelConsumptionRatioOfAutoMove = 1; // 自動移動時の燃料消費率
    private int _fuelConsumption; // 燃料消費量
    public bool isClear;//クリアしたかどうかのフラグ
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fuel = maxFuel;
        _rigidbody = GetComponent<Rigidbody>();
        _notNeedToRotate = true;
        isManipulating = false;
        isClear = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraDirection = followingVirtualCamera.transform.forward;
        
        if (canMove)
        {
            _horizontal = Input.GetAxis("Horizontal");
            _vertical = Input.GetAxis("Vertical");
            
            Vector3 headingDirection = cameraDirection * _vertical + followingVirtualCamera.transform.right * _horizontal;//向かう方向
            headingDirection.Normalize();

            if (_vertical != 0)
            {
                _notNeedToRotate = CheckRotation(headingDirection);//回転判定
            }

            if (!_notNeedToRotate)
            {
                Quaternion targetRotation = new Quaternion();
                targetRotation.SetFromToRotation(transform.forward, headingDirection);//向かう方向の回転

                transform.rotation =
                    Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
            
            if(_horizontal != 0 || _vertical != 0)
            {
                isManipulating = true; // 操作中ならフラグを立てる
            }
            else
            {
                isManipulating = false;
            }
        }
        else
        {
            _horizontal = 0f;
            _vertical = 0f;
        }
    }

    void FixedUpdate()
    {
        if (canMove && _notNeedToRotate)
        {
            Vector3 cameraDirection= followingVirtualCamera.transform.forward;//カメラの見ている方向からXZ平面の単位ベクトルを取得
            Vector3 moveDirection = cameraDirection * _vertical + followingVirtualCamera.transform.right * _horizontal;//キー入力から移動方向を決定
            moveDirection *= Time.fixedDeltaTime;
            
            _rigidbody.linearVelocity = moveDirection * speed;
            _fuelConsumption = Mathf.RoundToInt(Mathf.Abs(_horizontal) + Mathf.Abs(_vertical));
            _fuelConsumption *= fuelConsumptionRatioOfManipulation; // 燃料消費量を計算
            fuel -= _fuelConsumption;
            //Debug.Log(fuel);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == collisionTarget)
        {
            //Debug.Log("Hit");
            isClear = true;
            //Time.timeScale = 0f;
        }
    }

    private bool CheckRotation(Vector3 direction)
    {
        float theta = Vector3.Angle(transform.forward, direction);
        Debug.Log("theta: " + theta);
        
        return theta <= 5f;//5度以内なら回転しない
    }
}
