using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public abstract class BaseBall : MonoBehaviour
{
	public static float BaseSpeed = 8f;
	public MovementData MovementData { get; protected set; }
	protected float ballRadius;
	protected CircleCollider2D ccollider;

	protected LayerMask collidableLayermask;
	protected LayerMask playerAndColliderLayermask;

	protected Collider2D lastUpdateCollider = null;

	public float Rotation {
		get
		{
			return MovementData.Rotation;
		}
	}

	public bool IsBigBall { 
		get {
			return ballRadius > 0.35f;
		}
	}

	protected virtual void SetBallRadius(float radius) {
		ccollider = ccollider ?? GetComponent<CircleCollider2D>();
		float scale = radius / ccollider.radius;
		transform.localScale = new Vector2(scale, scale);
		ballRadius = radius;
	}

	public virtual void Start() {
		ccollider = GetComponent<CircleCollider2D>();
		SetBallRadius(0.25f);
		collidableLayermask = 1 << LayerMask.NameToLayer("Collidable");
		playerAndColliderLayermask = collidableLayermask | 1 << LayerMask.NameToLayer("Player");
	}

	public abstract void FixedUpdate();

	protected IEnumerator ChangeBallSize(float target, float time = 1f, YieldInstruction yield = null) {
		float startTime = Time.time;
		float startSize = ballRadius;
		var wait = yield ?? new WaitForEndOfFrame();
		while(Time.time - startTime < time + 0.1f) {
			SetBallRadius(Mathf.Lerp(startSize, target, (Time.time - startTime) / time));
			yield return wait;
		}
	}
}
