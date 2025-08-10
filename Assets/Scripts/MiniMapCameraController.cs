using UnityEngine;

public class MiniMapCameraController : MonoBehaviour
{
    public Probe probe;
    private readonly float _cameraDistance = 30f;

    void Start()
    {
        Vector3 probePosition = probe.transform.position;
        transform.position = probePosition + Vector3.up * _cameraDistance;
        transform.LookAt(probePosition);
    }

    void Update()
    {
        Vector3 probePosition = probe.transform.position;
        transform.position = probePosition + Vector3.up * _cameraDistance;
        transform.LookAt(probePosition);
    }
}
