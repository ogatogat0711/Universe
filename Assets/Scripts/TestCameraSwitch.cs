using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class TestCameraSwitch : MonoBehaviour
{
    public PlayableDirector crossFadeDirector; // クロスフェードのPlayableDirector
    public CinemachineVirtualCameraBase fromVirtualCamera;
    public CinemachineVirtualCameraBase toVirtualCamera;

    void Start()
    {
        fromVirtualCamera.Priority = 10;
        toVirtualCamera.Priority = 0;
        crossFadeDirector.Stop();
    }

    public void OnClick()
    {
        crossFadeDirector.time = 0f;
        crossFadeDirector.Play();
        fromVirtualCamera.Priority = 0;
        toVirtualCamera.Priority = 10;
        
    }
}
