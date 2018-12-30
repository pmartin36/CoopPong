using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HideOnPress : MonoBehaviour, IButtonEffected {
	private SpriteRenderer spriteRenderer;
	private Collider2D collider2d;

	private bool Enabled = false;
	Coroutine waitingForEnable;

	public float Amount { get; set; }
	public ButtonLocation[] PositiveActors;
	public ButtonLocation[] NegativeActors;

	public void AddActor(ButtonLocation location, float amount) {
		if (PositiveActors.Any(a => a == location)) {
			Amount += amount;
		}
		else if (NegativeActors.Any(a => a == location)) {
			Amount -= amount;
		}
	}

	private void SetEnabled(bool enabled) {
		if(Enabled != enabled) {
			Enabled = enabled;
			if (!enabled) {
				if(waitingForEnable != null) {
					StopCoroutine(waitingForEnable);
				}
				SetProps();
			}
			else {
				waitingForEnable = StartCoroutine(WaitForEnable());
			}		
		}
	}

	private void SetProps() {
		spriteRenderer.color = new Color(1, 1, 1, Enabled ? 1 : 0.5f);
		collider2d.enabled = Enabled;
	}

	public void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		collider2d = GetComponent<Collider2D>();
		SetProps();
	}

	public void LateUpdate() {
		SetEnabled(Amount > 0.5f);
		Amount = 0;
	}

	private IEnumerator WaitForEnable() {
		var layermask = LayerMask.NameToLayer("Ball");
		while( Physics2D.IsTouchingLayers(collider2d, layermask) ) {
			yield return new WaitForEndOfFrame();
		}
		SetProps();
	}
}
