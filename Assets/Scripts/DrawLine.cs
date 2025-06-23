using UnityEngine;
using UnityEngine.EventSystems;

public class DrawLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Camera upperCamera;//上方カメラ

    private int _positionCount = 0;//点の数
    private float _interval = 1f; // 点の間隔
    private bool _isDrawing = false; // 描画中かどうか

    // Update is called once per frame
    void Update()
    {
        // UI要素の上でマウスがクリックされている場合、描画を無効化
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isDrawing = false;
            }
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            // 左クリックが押されている間、描画を開始
            _isDrawing = true;
            _positionCount= 0; // 点の数をリセット
            lineRenderer.positionCount = _positionCount;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // 左クリックが離されたら描画を終了
            _isDrawing = false;
        }

        if (_isDrawing)
        {
            // 描画中の場合、マウスの位置を取得して点を追加
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = upperCamera.transform.position.y;// 上方カメラのY座標を基準にする
            Vector3 worldPosition = upperCamera.ScreenToWorldPoint(mousePosition);// マウス位置をワールド座標に変換
            worldPosition.y = 0;// Y座標を0に設定して平面上に制限
            SetPosition(worldPosition);
        }
            
    }
    
    private void SetPosition(Vector3 position)
    {
        if (!CheckPosition(position))// 前の点との距離が間隔以上でない場合は追加しない
        {
            return;
        }

        _positionCount++;
        lineRenderer.positionCount = _positionCount;
        lineRenderer.SetPosition(_positionCount - 1, position);
    }

    private bool CheckPosition(Vector3 position)
    {
        if (_positionCount == 0)
        {
            return true; // 最初の点は常に追加
        }

        Vector3 lastPosition = lineRenderer.GetPosition(_positionCount - 1);// 最後の点の位置を取得
        float distance = Vector3.Distance(lastPosition, position);// 前の点との距離を計算
        
        return distance >= _interval; // 前の点との距離が間隔以上ならtrue
    }
}
