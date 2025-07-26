using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CelestialBodyData
{
    public string bodyName;
    public bool isSpinOrbital;
    public GameObject orbitalCentralObject;
    public float orbitalRadius;
    public float orbitalCycle;
    public bool isSpinItself;
    public float gravitationCoefficient;
}

[CreateAssetMenu(fileName = "CelestialBodySetting", menuName = "Scriptable Objects/CelestialBodySetting")]
public class CelestialBodySetting : ScriptableObject
{
    public List<CelestialBodyData> dataList;
}
