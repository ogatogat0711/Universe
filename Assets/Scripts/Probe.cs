using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class Probe : MonoBehaviour
{
    //public Transform probeHead;
    public float speed = 15f;
    public float mouseSensitivity = 100f;
    private Rigidbody _rigidbody;
    private Vector3 _velocity;
    private float _horizontal;
    private float _vertical;
    private float _mouseX;
    private float _mouseY;
    private float _pitch;
    private Vector3 _forwardDirection;
    private float _rotationSpeed = 50f;//回転速度
    //public Camera followingCamera;
    public CinemachineVirtualCameraBase followingVirtualCamera;
    public CinemachineVirtualCameraBase fpsCamera; // FPSカメラ
    public GameObject collisionTarget;
    public bool canMove;//移動可能かどうかのフラグ
    public bool isManipulating; // 操作中かどうかのフラグ
    private bool _needToRotate;//回転が必要かのフラグ
    public int fuel; // 燃料
    public int maxFuel = 100;// 最大燃料
    public int fuelConsumptionRatioOfManipulation = 3; // 操作時の燃料消費率
    public int fuelConsumptionRatioOfAutoMove = 1; // 自動移動時の燃料消費率
    private int _fuelConsumption; // 燃料消費量
    public bool isClear;//クリアしたかどうかのフラグ
    public float damagePercentage;//損害率
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fuel = maxFuel;
        _rigidbody = GetComponent<Rigidbody>();
        _needToRotate = false;
        isManipulating = false;
        isClear = false;
        _forwardDirection = transform.forward;
        _pitch = 0f;
        damagePercentage = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            _horizontal = Input.GetAxis("Horizontal");
            _vertical = Input.GetAxis("Vertical");
            
            if(_horizontal != 0 || _vertical != 0)
            {
                isManipulating = true; // 操作中ならフラグを立てる

               _forwardDirection = followingVirtualCamera.transform.forward;
            }
            
            else
            {
                isManipulating = false;
            }
            
            
            _needToRotate = CheckRotation(_forwardDirection);//回転が必要かどうかをチェック

            if (_needToRotate)
            {
                Quaternion nextRotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(_forwardDirection), _rotationSpeed * Time.deltaTime);
                
                transform.rotation = nextRotation; // ゆっくり回転
            }
            
        }
        else if (fpsCamera.IsLive) // FPSカメラがアクティブでいるとき
        {
            _mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            _mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            transform.Rotate(Vector3.up * _mouseX); // 水平方向の回転
            
            _pitch -= _mouseY;
            _pitch = Mathf.Clamp(_pitch, -90f, 90f);

            fpsCamera.transform.rotation = Quaternion.Euler(_pitch, transform.eulerAngles.y, 0f);
        }
        
        else
        {
            _horizontal = 0f;
            _vertical = 0f;
        }
    }
    

    void FixedUpdate()
    {
        if (canMove && !_needToRotate)
        {
            //回転中は操作できない
            Vector3 cameraDirection= followingVirtualCamera.transform.forward;//カメラの見ている方向からXZ平面の単位ベクトルを取得
            Vector3 moveDirection = cameraDirection * _vertical + transform.right * _horizontal;//キー入力から移動方向を決定
            moveDirection *= Time.fixedDeltaTime;

            if (_vertical != 0f || _horizontal != 0f)
            {
                ResetInertia();//慣性をリセット
            }
            
            _rigidbody.linearVelocity = moveDirection * speed;
            
            //Debug.Log("current velocity: " + _rigidbody.linearVelocity);
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
        //Debug.Log("theta: " + theta);
        
        return theta > 0.5f;//0.5度より大きければ回転する
    }

    public void ResetInertia()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void SetForwardDirection(Vector3 direction)
    {
        _forwardDirection = direction;
    }
}
