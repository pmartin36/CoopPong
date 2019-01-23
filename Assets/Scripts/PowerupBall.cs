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

	protected override void HandlePlayerCollision(float dot, Vector2 normal, RaycastHit2D hit, Vector3 lastMoveDirection) {
		Player p = hit.collider.GetComponentInParent<Player>();
		// add powerup for player
		p.AddPowerup(Powerup);
		this.gameObject.Destroy();
	}
}
