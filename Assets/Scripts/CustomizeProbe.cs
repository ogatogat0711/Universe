using UnityEngine;

public class CustomizeProbe : MonoBehaviour
{
    public GameObject probe;
    public float rotationSpeed;

    void Update()
    {
        probe.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
