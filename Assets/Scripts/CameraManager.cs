using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public static void ChangeCamera(Camera oldCamera, Camera newCamera)
    {
        if (oldCamera != null)
        {
            oldCamera.gameObject.SetActive(false);
        }
        
        if (newCamera != null)
        {
            newCamera.gameObject.SetActive(true);
        }
    }
}
