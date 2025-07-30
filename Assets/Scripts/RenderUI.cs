using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class RenderUI : MonoBehaviour
{
    public Camera mainCamera;
    public CinemachineVirtualCameraBase upperVirtualCamera;
    public Camera uiCamera;
    // public LayerMask layerMask;
    // public Material unlitMaterial;
    private GameObject _lastHit;
    public InformationWindow infoWindow;
    public GameObject guideForInformation;

    void Start()
    {
        guideForInformation.SetActive(false);
    }

    void Update()
    {
        if (DrawLine.isDrawing) return; //描画中は何もしない
        if (upperVirtualCamera.IsLive)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject.CompareTag("CelestialBody"))
                {
                    _lastHit = hit.collider.gameObject;
                    var cb = _lastHit.GetComponent<CelestialBody>();
                    var data = cb.GetCelestialBodyData();

                    if (!InformationWindow.isShowing)
                    {
                        infoWindow.SetInformation(data);
                        guideForInformation.SetActive(true);
                        
                        if (Input.GetKeyDown(KeyCode.Q) && _lastHit != null)
                        {
                            StartCoroutine(Show());
                        }
                    }
                    
                }
            }
            else
            {
                guideForInformation.SetActive(false);
            }
            
        }
    }

    IEnumerator ShowInUI()
    {
        foreach (Transform c in uiCamera.transform)
        {
            Destroy(c.gameObject);//UIカメラの子オブジェクトを一旦全て消す
        }
        
        GameObject clone = Instantiate(_lastHit);//クローン生成
        float radius = clone.transform.localScale.magnitude;
        clone.transform.SetParent(uiCamera.transform,false); // UIカメラの子オブジェクトに設定
        clone.transform.localPosition = Vector3.forward * (3f + radius);// UIカメラからの距離を設定
        clone.transform.localRotation = Quaternion.identity;

       SetLayerRecursively(clone,LayerMask.NameToLayer("Celestial"));
        
        yield return null;
    }

    IEnumerator Show()
    {
        yield return infoWindow.ShowInfoWindow();
        yield return ShowInUI();
    }
    
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
