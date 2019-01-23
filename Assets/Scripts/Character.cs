using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

	private void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void SetColor(Color c) {
		spriteRenderer.color = c;
	}
}
