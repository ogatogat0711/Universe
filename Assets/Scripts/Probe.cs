using UnityEngine;

public class Probe : MonoBehaviour
{
    public float speed = 15f;
    private Rigidbody _rigidbody;
    private Vector3 _velocity;
    private float _horizontal;
    private float _vertical;
    private Camera _camera;
    public GameObject collisionTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        Vector3 cameraDirection= Vector3.Scale(_camera.transform.forward, new Vector3(1, 0, 1)).normalized;//カメラの見ている方向からXZ平面の単位ベクトルを取得
        Vector3 moveDirection = cameraDirection * _vertical + _camera.transform.right * _horizontal;//キー入力から移動方向を決定
        _rigidbody.linearVelocity = moveDirection * speed;
        
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
