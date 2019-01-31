using System;
using System.Collections;
using UnityEngine;

public class SmallEnemy : BaseEnemy, IEffector {
	public StatusEffect Effect { get => StatusEffect.Shrunk; }

	public event EventHandler Destroyed;

	public void OnEnable() {
		Spawning = true;
		SelectTarget();
		StartCoroutine(SpawnOnDelay());
	}

	IEnumerator SpawnOnDelay() {
		yield return new WaitForSeconds(1f);
		SpawnComplete();
	}

	public void SelectTarget() {
		
	}

	private void SpawnComplete() {
		Spawning = false;
		gameObject.layer = LayerMask.NameToLayer("Target");
		var lm = GameManager.Instance.LevelManager;
		lm.RightPlayer.AddStatusEffect(this);
		lm.LeftPlayer.AddStatusEffect(this);
	}

	public void OnDestroy() {
		Destroyed?.Invoke(this, null);
	}
}