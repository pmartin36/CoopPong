using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

[Serializable]
public class LaserEnemyProperties : SpawnProperties {
	public Vector3 RestingPosition;

	public LaserEnemyProperties() { }
	public LaserEnemyProperties(Vector3 v) {
		RestingPosition = v;
	}

	public override void Render() {
		base.Render();
		RestingPosition = EditorGUILayout.Vector3Field("Resting Position", RestingPosition, GUILayout.Width(750));
	}
}


