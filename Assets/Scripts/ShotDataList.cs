using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShotData
{
    public int shotID;
    public GameObject shotPrefab;
    public int speed;
    public int attack;
    public float shotInterval;
    public int fuelConsumptionOfShot;
}

[CreateAssetMenu(fileName = "ShotDataList", menuName = "Scriptable Objects/ShotDataList")]
public class ShotDataList : ScriptableObject
{
    public List<ShotData> shotDataList;

    public ShotData FindShotDataById(int id)
    {
        foreach (ShotData shotData in shotDataList)
        {
            if (shotData.shotID == id)
            {
                return shotData;
            }
        }

        return null;
    }
}
