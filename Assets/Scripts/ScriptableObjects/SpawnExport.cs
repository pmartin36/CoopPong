using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

[Serializable]
public class SpawnExport : ScriptableObject
{
    public List<SpawnObjectInfo> SpawnInfo;

	public void OnEnable() {
		if (SpawnInfo == null)
			SpawnInfo = new List<SpawnObjectInfo>();
	}

	public void Save() {
		foreach(var s in SpawnInfo) {
			s.Save();
		}
		EditorUtility.SetDirty(this);
	}
}
