using UnityEngine;
using System.Collections;
using System.Linq;

public enum Powerup {
	BigBall,
	Laser,
	Remote
}

public class PowerupBall : BaseBall {

	public Powerup Powerup;

	public void Init(MovementData md) {
		MovementData = new MovementData(md);
		MovementData.MovementDirection *= -1f;
		transform.position = MovementData.Position;

		if(Powerup == Powerup.BigBall) {
			StartCoroutine(ChangeBallSize(0.5f));
		}
	}

	// Update is called once per frame
	public override void FixedUpdate() {
		var lastMovement = new MovementData(MovementData);
		MovementData.Update(Time.fixedDeltaTime);

		transform.Rotate(0, 0, Rotation * Time.fixedDeltaTime);
		RaycastHit2D hitBetweenLastMove = Physics2D.CircleCastAll(transform.position, ballRadius,
			lastMovement.ActualMovementDirection, lastMovement.ActualMoveSpeed, playerAndColliderLayermask).FirstOrDefault(h => h.collider != lastUpdateCollider);

		if(hitBetweenLastMove.collider != null) {
			if(hitBetweenLastMove.collider.tag == "Player") {
				Player p = hitBetweenLastMove.collider.GetComponent<Player>();
				// add powerup for player
				p.AddPowerup(Powerup);
				this.gameObject.Destroy();
			}
			else {
				Vector2 actualMoveDirection = lastMovement.ActualMovementDirection;
				Vector2 normal = hitBetweenLastMove.normal;
				float dot = Vector2.Dot(actualMoveDirection, normal);
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
}
