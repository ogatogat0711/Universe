using UnityEngine;
using UnityEngine.Serialization;

public class CelestialBody : MonoBehaviour
{
    public bool isSpinOrbital;//公転するかのフラグ
    public bool wasSetSpinOrbital;//公転が設定されたかのフラグ
    public GameObject orbitalCentralObject;//公転中心
    public float orbitalRadius = 15f;//公転半径
    public float orbitalCycle = 27f;//公転周期
    private float _orbitalAngle;//公転角度

    public bool isSpinItself;//自転するかのフラグ
    private float _spinAngle;//自転角度
    
    public float simulationSpeed = 0.01f;//シミュレーション速度(デバッグ用)
    
    public bool isGravitation;//万有引力を作用するかのフラグ
    public float gravitationCoefficient = 1f;//万有引力の係数(GmM)
    public Probe gravitationTargetObject;//万有引力を作用するオブジェクト
    private Rigidbody _gravitationTarget;//作用先
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _orbitalAngle = Mathf.Deg2Rad * 90f; //公転角度の初期値を90度に設定（ラジアンに変換）
        _spinAngle = Mathf.Deg2Rad * 90f;//自転角度の初期値を90度に設定（ラジアンに変換）

        if (isSpinOrbital)//公転をする天体の初期位置設定
        {
            float x = orbitalRadius;//sin(90)=1のため
            float z = 0; //cos(90)=0のため
            
            Vector3 orbitalCentralPos = orbitalCentralObject.transform.position;
            transform.position = orbitalCentralPos + new Vector3(x, 0, z);//該当天体の初期値
        }

        _gravitationTarget = gravitationTargetObject.gameObject.GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        if (isSpinItself)//自転運動
        {
            _spinAngle = Time.deltaTime * 360 * simulationSpeed;
            transform.RotateAround(transform.position, Vector3.up, _spinAngle);
        }

        if (isSpinOrbital) //公転運動
        {
            _orbitalAngle -= Time.deltaTime * 2 * Mathf.PI / orbitalCycle * simulationSpeed;
            float x = Mathf.Sin(_orbitalAngle) * orbitalRadius;
            float z = Mathf.Cos(_orbitalAngle) * orbitalRadius;

            Vector3 orbitalCentralPos = orbitalCentralObject.transform.position;
            transform.position = orbitalCentralPos + new Vector3(x, 0, z);
        }
    }

    void FixedUpdate()
    {
        if (isGravitation && !gravitationTargetObject.fpsCamera.IsLive)
        {
            Vector3 gravityDirection = transform.position - _gravitationTarget.transform.position;//万有引力の作用方向のベクトル
            float distanceSquared = Mathf.Pow(gravityDirection.magnitude, 2.0f); //ベクトルの大きさの2乗

            float force = gravitationCoefficient / distanceSquared;//GmM/r^2
            force *= Time.fixedDeltaTime;

            _gravitationTarget.AddForce(gravityDirection.normalized * force, ForceMode.Force);//万有引力を作用
        }
        
    }
}
