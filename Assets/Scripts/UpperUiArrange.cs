using System.Collections;
using DG.Tweening;
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
    private Coroutine _arrowAnimationCoroutine;

    void Start()
    {
        Vector3 screenProbePosition = mainCamera.WorldToScreenPoint(probe.transform.position);
        Vector3 screenGoalPosition = mainCamera.WorldToScreenPoint(probe.collisionTarget.transform.position);

        Vector2 uiProbePosition, uiGoalPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(upperCanvas.GetComponent<RectTransform>(),
            screenProbePosition, null, out uiProbePosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(upperCanvas.GetComponent<RectTransform>(),
            screenGoalPosition, null, out uiGoalPosition);

        uiProbePosition += Vector2.up * 100f;
        uiGoalPosition += Vector2.up * 100f;
        
        arrow.anchoredPosition = uiProbePosition;
        goalFlag.anchoredPosition = uiGoalPosition;
        
        arrow.gameObject.SetActive(true);
        goalFlag.gameObject.SetActive(true);

        _arrowAnimationCoroutine = null;
    }

    void Update()
    {
        if (!upperVirtualCamera.IsLive) return;//上方カメラではないときはなにもしない

        if (arrow.gameObject.activeSelf && _arrowAnimationCoroutine == null)
        {
            _arrowAnimationCoroutine = StartCoroutine(ArrowAnimation());
        }
    }

    private IEnumerator ArrowAnimation()
    {
        Vector2 initialPosition = arrow.anchoredPosition;
        arrow.DOAnchorPos(initialPosition + Vector2.up * 50f, 0.6f).SetEase(Ease.OutCirc);
        yield return new WaitForSeconds(0.6f);
        arrow.DOAnchorPos(initialPosition, 0.6f).SetEase(Ease.InCirc);
        yield return new WaitForSeconds(0.6f);

        _arrowAnimationCoroutine = null;
    }
}
