using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
	public static float BaseLinePosition = 16.5f;

	private PlayerSide Side;
	private float LinePosition;

	public CapsuleCollider2D capsuleCollider;
	private SpriteRenderer[] spriteRenderers;
	private Transform body;
	private Transform topCap;
	private Transform bottomCap;

	private void Awake() {
		spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

		foreach (Transform t in transform) {
			if (t.CompareTag("PlayerCapTop")) {
				topCap = t;
			}
			else if (t.CompareTag("PlayerBody")) {
				body = t;
			}
			else if (t.CompareTag("PlayerCapBottom")) {
				bottomCap = t;
			}
		}

		capsuleCollider = GetComponent<CapsuleCollider2D>();
	}

	public void Init(Player p) {
		Side = p.Side;
		LinePosition = BaseLinePosition * (Side == PlayerSide.Left ? -1f : 1f);
		transform.position = new Vector3(LinePosition, p.transform.position.y, this.transform.position.z);
	}

	public void SetColor(Color c) {
		foreach (SpriteRenderer s in spriteRenderers) {
			s.color = c;
		}
	}

	public void SetInPlay(bool inPlay) {
		capsuleCollider.enabled = inPlay;
	}

	public void SetWidth(float colliderSize, float bodySize) {
		capsuleCollider.size = new Vector2(capsuleCollider.size.x, colliderSize);
		body.localScale = new Vector2(body.localScale.x, bodySize);
		topCap.localPosition = new Vector2(0, bodySize);
		bottomCap.localPosition = new Vector2(0, -bodySize);
	}
}
