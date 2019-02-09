using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DotType {
	Powerup,
	Required,
	Golden,
	Enemy
}

public class Dot : MonoBehaviour
{
	public DotType DotType;
	public float ColorMultiplier;

	protected Color color;
	protected SpriteRenderer sr;

	protected Animator anim;

    public virtual void Start() {
		sr = GetComponent<SpriteRenderer>();
		switch (DotType) {
			case DotType.Powerup:
				color = Color.white;
				break;
			case DotType.Required:
				GameManager.Instance.LevelManager.AddRequiredDot();
				color = Color.green;
				break;
			case DotType.Golden:
				color = new Color(1, 0.97f, 0.4f);
				break;
			case DotType.Enemy:
				color = Color.red;
				break;
			default:
				break;
		}
		anim = GetComponentInParent<Animator>();
		StartCoroutine(StartAnimationAfterDelay());
	}

    void Update()
    {
        sr.color = color * ColorMultiplier;
    }

	public void TryDestroy(GameObject collision) {
		OnDestroyEffect(collision);
		this.gameObject.Destroy();
	}

	public virtual void OnDestroyEffect(GameObject collision) {
		GameManager.Instance.LevelManager.DotCollected(this.DotType);
	}

	private void OnDestroy() {
		
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if(collision.CompareTag("Ball")) {
			TryDestroy(collision.gameObject);
		}
	}

	IEnumerator StartAnimationAfterDelay() {
		yield return new WaitForSeconds(UnityEngine.Random.value);
		anim.Play("Dot");
	}
}
