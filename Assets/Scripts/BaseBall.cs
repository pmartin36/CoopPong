using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	protected abstract void HandlePlayerCollision(float dot, Vector2 normal, RaycastHit2D hit, Vector3 lastMoveDirection);

	public virtual void FixedUpdate() {
		var lastMovement = new MovementData(MovementData);
		MovementData.Update(Time.fixedDeltaTime);

		transform.Rotate(0, 0, Rotation * Time.fixedDeltaTime);
		RaycastHit2D hitBetweenLastMove = Physics2D.CircleCastAll(transform.position, ballRadius,
			lastMovement.ActualMovementDirection, lastMovement.ActualMoveSpeed, playerAndColliderLayermask).FirstOrDefault(h => h.collider != lastUpdateCollider);

		if (hitBetweenLastMove.collider != null) {
			Vector2 actualMoveDirection = lastMovement.ActualMovementDirection;
			Vector2 normal = hitBetweenLastMove.normal;
			float dot = Vector2.Dot(actualMoveDirection, normal);

			if (hitBetweenLastMove.collider.CompareTag("Player")) {
				HandlePlayerCollision(dot, normal, hitBetweenLastMove, lastMovement.ActualMovementDirection);
			}
			else {
				float msIncrease = hitBetweenLastMove.collider.GetComponent<IMoving>() != null ? 0f : 0.25f;
				if (dot < 0) {
					// Debug.Log($"Est - n: {normal}, d: {dot}, am: {actualMoveDirection} c: {ballHit.collider.gameObject.name}");
					MovementData.HandleNonPlayerCollision(dot, normal, Vector3.zero, msIncrease, hitBetweenLastMove.centroid);
				}
				lastUpdateCollider = hitBetweenLastMove.collider;
			}
		}

		transform.position = MovementData.Position;
	}

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
