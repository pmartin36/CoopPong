using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class SlowEnemyProperties : SpawnProperties
{
    public Vector3 RestingPosition;
    public bool Test;

    public SlowEnemyProperties() { }
    public SlowEnemyProperties(Vector3 v)
    {
        RestingPosition = v;
    }
}
