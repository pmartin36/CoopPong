using System;
using UnityEngine;

[Serializable]
public class SpawnProperties : ScriptableObject {

	public void OnEnable() {
		hideFlags = HideFlags.HideAndDontSave;
	}

	public virtual void Render() {

	}
}