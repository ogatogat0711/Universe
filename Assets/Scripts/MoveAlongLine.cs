using UnityEngine;
using UnityEngine.Serialization;

public class MoveAlongLine : MonoBehaviour
{
    public LineRenderer drawLine;
    public float moveSpeed = 5f;
    private float _transferringDistance = 0.2f;//次のインデックスに進む基準の距離
    public float maxDistance = 3f;//曲線から離れたとみなす最大距離
    public bool canAutoMove;//移動可能かどうかのフラグ.
    public bool isMoving;//移動中かどうかのフラグ.UIボタンで制御

    public int currentIndex;
    private Probe _probe;
    private Rigidbody _rigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _probe = GetComponent<Probe>();
        _rigidbody = _probe.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (isMoving && drawLine.positionCount > 1)
        {
            Vector3 target = drawLine.GetPosition(currentIndex + 1);//向かう先の座標
            Vector3 direction = (target - transform.position).normalized;//向かう方向
            
            _rigidbody.AddForce(direction * moveSpeed);
            _probe.fuel -= _probe.fuelConsumptionRatioOfAutoMove;

            if (Vector3.Distance(transform.position, target) < _transferringDistance)
            {
                //次のインデックスに移動
                currentIndex++;
                if (currentIndex >= drawLine.positionCount - 1)
                {
                    isMoving = false;//最後のインデックスまで行ったので終了
                }
            }

            float minDistance = GetMinDistanceFromLine();
            if (minDistance > maxDistance)
            {
                isMoving = false;//曲線から離れたため自動運転停止
                canAutoMove = false;
            }
        }
        
    }

    private float GetMinDistanceFromLine()
    {
        float minDistance = float.MaxValue;
        for (int i = 0; i < drawLine.positionCount; i++)
        {
            float distance = Vector3.Distance(transform.position, drawLine.GetPosition(i));
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }
        
        return minDistance;
    }
}
