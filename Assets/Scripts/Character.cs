using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private SkinnedMeshRenderer[] meshRenderers;

	private void Start() {
		meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
	}

	public void SetColor(Color c) {
		foreach(SkinnedMeshRenderer r in meshRenderers) {
			r.material.color = c;
		}
	}
}
