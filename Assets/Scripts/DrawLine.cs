using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class DrawLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    //public Camera upperCamera;//上方カメラ
    public CinemachineVirtualCameraBase upperVirtualCamera;// 上方カメラの仮想カメラ
    private Camera _mainCamera;//描画用のメインカメラ

    private int _positionCount = 0;//点の数
    public int maxPositionCount = 90; // 最大点数
    private float _interval = 1f; // 点の間隔
    public static bool IsDrawing; // 描画中かどうか

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        
        if (Input.GetMouseButtonDown(0) && upperVirtualCamera.IsLive && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            // 左クリックとCtrlキーが押されている間、描画を開始.ただし,上方カメラの時のみ
            IsDrawing = true;
            _positionCount= 0; // 点の数をリセット
            lineRenderer.positionCount = _positionCount;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // 左クリックが離されたら描画を終了
            IsDrawing = false;
        }

        if (IsDrawing)
        {
            // 描画中の場合、マウスの位置を取得して点を追加
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = upperVirtualCamera.transform.position.y;// 上方カメラのY座標を基準にする
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);// マウス位置をワールド座標に変換
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

        if (_positionCount > maxPositionCount)
        {
            return false;// 点の数が最大を超えたら追加しない
        }

        Vector3 lastPosition = lineRenderer.GetPosition(_positionCount - 1);// 最後の点の位置を取得
        float distance = Vector3.Distance(lastPosition, position);// 前の点との距離を計算
        
        return distance >= _interval; // 前の点との距離が間隔以上ならtrue
    }
    
}
