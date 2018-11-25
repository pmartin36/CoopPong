using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class JailEnemyProperties : SpawnProperties
{
    public Vector3 RestingPosition;

    public JailEnemyProperties() { }
    public JailEnemyProperties(Vector3 v)
    {
        RestingPosition = v;
    }
}
