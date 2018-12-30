using UnityEngine;
using System.Collections;
using System;

public class JailBar : MonoBehaviour {
	JailEnemy parentJailer;

	public void JailerDestroyed(object sender, EventArgs e){
		this.gameObject?.Destroy();
	}

	public void SignUpForDestroyedEvent(JailEnemy e) {
		parentJailer = e;
		e.Destroyed += JailerDestroyed;
	}

	private void OnDestroy() {
		parentJailer.Destroyed -= JailerDestroyed;
	}
}
