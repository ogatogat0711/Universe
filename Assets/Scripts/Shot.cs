using System;
using UnityEngine;

public class Shot : MonoBehaviour
{
    public int speed;
    public int attack;
    private Probe _probe;
    public int maxDistance;
    public float shotInterval;
    public int fuelConsumptionRatioOfShot = 2;

    void Start()
    {
        _probe = FindAnyObjectByType<Probe>();
        if (_probe == null)
        {
            throw new MissingComponentException("Probe not found");
        }
    }
    
    void Update()
    {
        if (Vector3.Distance(transform.position, _probe.transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    public void InitShot(ShotData shotData)
    {
        speed = shotData.speed;
        attack = shotData.attack;
        shotInterval = shotData.shotInterval;
        fuelConsumptionRatioOfShot = shotData.fuelConsumptionOfShot;
    }
}
