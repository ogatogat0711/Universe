using System;
using UnityEngine;

public class MeteorZone : MonoBehaviour
{
    private Probe _probe;
    public MeteorRoot meteorRoot; // MeteorRootを参照するための変数

    void Start()
    {
        _probe = meteorRoot.probe;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.Equals(_probe.gameObject))
        {
            meteorRoot.isSpawning = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.Equals(_probe.gameObject))
        {
            meteorRoot.isSpawning = false;
        }
    }
}
