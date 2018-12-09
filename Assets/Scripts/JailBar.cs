using UnityEngine;
using System.Collections;
using System;

public class JailBar : MonoBehaviour {
	public void JailerDestroyed(object sender, EventArgs e){
		this.gameObject?.Destroy();
	}
}
