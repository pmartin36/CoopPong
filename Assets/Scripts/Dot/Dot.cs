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
		switch (DotType) {		
			case DotType.Required:
				// remove from required
				break;
			case DotType.Golden:
				// award bonus star
				break;
			case DotType.Enemy: // spawn enemy
			case DotType.Powerup: // chance to drop powerup
				// handled in derived classes
				break;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if(collision.CompareTag("Ball")) {
			TryDestroy(collision.gameObject);
		}
	}

	IEnumerator StartAnimationAfterDelay() {
		yield return new WaitForSeconds(Random.value);
		anim.Play("Dot");
	}
}
