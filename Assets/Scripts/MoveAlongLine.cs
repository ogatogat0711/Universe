using UnityEngine;
using UnityEngine.Serialization;

public class MoveAlongLine : MonoBehaviour
{
    public LineRenderer drawLine;
    public float moveSpeed = 5f;
    private float _transferringDistance = 0.5f;//次のインデックスに進む基準の距離
    public float maxDistance = 1.5f;//曲線から離れたとみなす最大距離
    public bool canAutoMove;//移動可能かどうかのフラグ.
    public bool isMoving;//移動中かどうかのフラグ
    public bool wasFarAway;//曲線から離れたかどうかのフラグ
    
    public int currentIndex;
    private float _nearLineTimer = 0f;
    public float reenableAutoMoveTime = 3f;
    private Probe _probe;
    private Rigidbody _rigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _probe = GetComponent<Probe>();
        _rigidbody = _probe.GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        var (minDistance, minIndex) = GetMinDistanceAndIndexFromLine();//現在位置から最も近い点までの距離とインデックスを取得
        if (!canAutoMove && wasFarAway)
        {
            if (minDistance <= maxDistance)
            {
                _nearLineTimer += Time.fixedDeltaTime;//近い点にいる時間を計測
                Debug.Log(_nearLineTimer);
                if (_nearLineTimer >= reenableAutoMoveTime)
                {
                    //曲線に復帰準備完了
                    //Debug.Log("復帰可能");
                    canAutoMove = true;
                    
                }
            }
            else
            {
                _nearLineTimer = 0f;
            }

            return;
        }

        if (canAutoMove && wasFarAway && Input.GetKeyDown(KeyCode.R))
        {
            //最近傍のインデックスから曲線に復帰
            currentIndex = minIndex;
            _nearLineTimer = 0f;
            wasFarAway = false;
            isMoving = true;
            //Debug.Log("復帰");
        }
        
        if (isMoving && drawLine.positionCount > 1 && !wasFarAway)
        {
            if (minDistance >= maxDistance)
            {
                //曲線から離脱
                isMoving = false;
                canAutoMove = false;
                wasFarAway = true;
                //Debug.Log("ライン離脱");
                return;
            }
            
            if (currentIndex >= drawLine.positionCount - 1)
            {
                isMoving = false;//最後のインデックスまで行ったので終了
            }
            
            Vector3 target = drawLine.GetPosition(currentIndex + 1);//向かう先の座標
            Vector3 direction = (target - transform.position).normalized;//向かう方向
            
            _rigidbody.AddForce(direction * moveSpeed);

            if (Vector3.Distance(transform.position, target) < _transferringDistance)
            {
                //次のインデックスに移動
                currentIndex++;
                _probe.fuel -= _probe.fuelConsumptionRatioOfAutoMove;//燃料を消費
                
            }
            
        }
        
    }

    private (float, int) GetMinDistanceAndIndexFromLine()
    {
        float minDistance = float.MaxValue;
        int minIndex = 0;
        for (int i = 0; i < drawLine.positionCount; i++)
        {
            float distance = Vector3.Distance(transform.position, drawLine.GetPosition(i));
            if (distance < minDistance)
            {
                minDistance = distance;
                minIndex = i;
            }
        }
        return (minDistance, minIndex);
    }
    
}
