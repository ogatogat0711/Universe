using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Level
{
    public int levelId;
    public string levelName;
    public Sprite thumbnail;
    public bool isUnlocked;
    public string sceneName;
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public List<Level> levelList;
    
}
