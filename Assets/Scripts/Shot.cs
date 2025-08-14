using System;
using UnityEngine;

public class Shot : MonoBehaviour
{
    public int speed;
    public int attack;
    private Probe _probe;
    private readonly int _maxDistance = 30;
    public float shotInterval;
    public int fuelConsumptionRatioOfShot = 2;
    public string shotName;
    public bool chargeable;
    public float chargeTime;
    public int chargeableBullets;

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
        if (Vector3.Distance(transform.position, _probe.transform.position) > _maxDistance)
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
        shotName = shotData.shotName;
        chargeable = shotData.chargeable;
        chargeableBullets = shotData.chargeableBullets;
        chargeTime = shotData.chargeTime;
    }
}
