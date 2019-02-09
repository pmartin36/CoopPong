using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalDot : Dot
{
	public PowerupBall Powerup;

	public override void OnDestroyEffect(GameObject collider) {
		base.OnDestroyEffect(collider);
		// drop powerup
		Powerup.transform.parent = null;
		Powerup.gameObject.SetActive(true);

		if(collider.CompareTag("Ball")) {
			var md = collider.GetComponent<BaseBall>()?.MovementData;
			Powerup.Init(md);
		}
		else {
			Powerup.Init(new MovementData(transform.position, Utils.AngleToVector(Random.value * 360)));
		}
	}
}
