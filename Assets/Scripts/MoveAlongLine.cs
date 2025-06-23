using UnityEngine;

public class MoveAlongLine : MonoBehaviour
{
    public LineRenderer drawLine;
    public float moveSpeed = 5f;
    private float _transferringDistance = 0.2f;//次のインデックスに進む基準の距離
    public float maxDistance = 3f;//曲線から離れたとみなす最大距離
    public bool canMove;//移動可能かどうかのフラグ.
    public bool isMoving;//移動中かどうかのフラグ.UIボタンで制御

    private int _currentIndex;
    private Rigidbody _rigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _currentIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (isMoving && drawLine.positionCount > 1)
        {
            Debug.Log("Moving along line! Current Index: " + _currentIndex);
            Vector3 target = drawLine.GetPosition(_currentIndex + 1);//向かう先の座標
            Vector3 direction = (target - transform.position).normalized;//向かう方向
            
            _rigidbody.AddForce(direction * moveSpeed);

            if (Vector3.Distance(transform.position, target) < _transferringDistance)
            {
                //次のインデックスに移動
                _currentIndex++;
                if (_currentIndex >= drawLine.positionCount - 1)
                {
                    isMoving = false;//最後のインデックスまで行ったので終了
                }
            }

            float minDistance = GetMinDistanceFromLine();
            if (minDistance > maxDistance)
            {
                isMoving = false;//曲線から離れたため自動運転停止
                canMove = false;
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

    //UIボタンから呼び出されるメソッド
    public void StartMoving()
    {
        if (Vector3.Distance(transform.position, drawLine.GetPosition(0)) < 1f)// 初期位置がラインの始点に近い場合のみ移動を開始
        {
            canMove = true;
            isMoving = true;
            _currentIndex = 0; // 移動開始時にインデックスをリセット
        }
    }
}
