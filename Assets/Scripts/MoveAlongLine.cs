using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class MoveAlongLine : MonoBehaviour
{
    public LineRenderer drawLine;
    public float moveSpeed = 5f;
    private float _transferringDistance = 0.5f;//次のインデックスに進む基準の距離
    public float maxDistance = 1.5f;//曲線から離れたとみなす最大距離
    public bool isEnableFollowing;
    public bool canAutoMove;//移動可能かどうかのフラグ.
    public bool isMoving;//移動中かどうかのフラグ
    public bool wasFarAway;//曲線から離れたかどうかのフラグ
    public bool isRecovering;//曲線に復帰中かどうかのフラグ
    public CinemachineVirtualCameraBase fpsCamera;
    
    public int currentIndex;
    public float nearLineTimer = 0f;
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
        if (!isEnableFollowing) return; //追従カメラが無効なときは何もしない
        
        var (minDistance, minIndex) = GetMinDistanceAndIndexFromLine();//現在位置から最も近い点までの距離とインデックスを取得
        if (!canAutoMove && wasFarAway)//離脱時
        {
            if (minDistance <= maxDistance)//曲線付近にいるとき
            {
                isRecovering = true;
                nearLineTimer += Time.fixedDeltaTime;//近い点にいる時間を計測
                //Debug.Log(_nearLineTimer);
                if (nearLineTimer >= reenableAutoMoveTime)//タイマーが必要時間以上経ったとき
                {
                    //曲線に復帰準備完了
                    //Debug.Log("復帰可能");
                    canAutoMove = true;
                    isRecovering = false;
                    
                }
            }
            else //曲線付近にいないとき
            {
                nearLineTimer = 0f;
                isRecovering = false;
            }

            return;
        }

        if (canAutoMove && wasFarAway && Input.GetKeyDown(KeyCode.R))//復帰準備完了後、Rキー押下時
        {
            //最近傍のインデックスから曲線に復帰
            currentIndex = minIndex;
            nearLineTimer = 0f;
            wasFarAway = false;
            isMoving = true;
            //Debug.Log("復帰");
        }

        if (isMoving && _probe.isManipulating)//Probeが操作中のとき
        {
            isMoving = false;//自動航行を停止
        }
        
        if (minDistance >= maxDistance)//曲線から距離が離れたとき
        {
            //曲線から離脱
            isMoving = false;
            canAutoMove = false;
            wasFarAway = true;
            //Debug.Log("ライン離脱");
            return;
        }
        
        if (isMoving && !wasFarAway)//自動航行中で非離脱時
        {
            if (fpsCamera.IsLive)
            {
                return;//fpsのときは何もしない
            }
            
            if (currentIndex >= drawLine.positionCount - 1)//インデックスが最後の点に達したとき
            {
                isMoving = false;//最後のインデックスまで行ったので終了
                return;
            }
            
            Vector3 target = drawLine.GetPosition(currentIndex + 1);//向かう先の座標
            Vector3 direction = (target - transform.position).normalized; //向かう方向
            direction *= Time.fixedDeltaTime;
            
            _rigidbody.AddForce(direction * moveSpeed);

            if (Vector3.Distance(transform.position, target) < _transferringDistance)//次のインデックスに進む距離を下回ったとき
            {
                //次のインデックスに移動
                currentIndex++;
                _probe.fuel -= _probe.fuelConsumptionRatioOfAutoMove;//燃料を消費
                
            }
            
        }
        
        if( !_probe.isManipulating && !isMoving && !wasFarAway && minDistance <= maxDistance && _probe.canMove)//操作はしたが離脱はしておらず、インデックスが異なるとき
        {
            currentIndex = minIndex; // 最も近い点のインデックスに設定
            isMoving = true;
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
