using Unity.Cinemachine;
using UnityEngine;

public class UpperUiArrange : MonoBehaviour
{
    public Probe probe;
    public Camera mainCamera;
    public CinemachineVirtualCameraBase upperVirtualCamera;
    public RectTransform arrow;
    public RectTransform goalFlag;
    public Canvas upperCanvas;

    void Update()
    {
        if (!upperVirtualCamera.IsLive) return;//上方カメラではないときはなにもしない
        
        Vector3 screenProbePosition = mainCamera.WorldToScreenPoint(probe.transform.position);
        Vector3 screenGoalPosition = mainCamera.WorldToScreenPoint(probe.collisionTarget.transform.position);

        Vector2 uiProbePosition, uiGoalPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(upperCanvas.GetComponent<RectTransform>(),
            screenProbePosition, null, out uiProbePosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(upperCanvas.GetComponent<RectTransform>(),
            screenGoalPosition, null, out uiGoalPosition);

        uiProbePosition += Vector2.up * 100f;
        uiGoalPosition += Vector2.up * 100f;

        float t = Mathf.PingPong(Time.time * 0.5f, 1f);
        float yOffset = Mathf.Sin(t * Mathf.PI * 2f) * 15f;
        
        uiProbePosition.y += yOffset;// 上下に揺れる効果を追加
        
        arrow.anchoredPosition = uiProbePosition;
        goalFlag.anchoredPosition = uiGoalPosition;
        
        if(!arrow.gameObject.activeSelf)
            arrow.gameObject.SetActive(true);
        
        if(!goalFlag.gameObject.activeSelf)
            goalFlag.gameObject.SetActive(true);
    }
}
