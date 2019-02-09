using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

[Serializable]
public class SmallEnemyProperties : SpawnProperties
{
	public Vector3 RestingPosition;
	public bool Test;

    public SmallEnemyProperties() { }
    public SmallEnemyProperties(Vector3 v)
    {
        RestingPosition = v;
    }

	public override void Render() {
		base.Render();
		RestingPosition = EditorGUILayout.Vector3Field("Resting Position", RestingPosition, GUILayout.Width(750));
	}
}
