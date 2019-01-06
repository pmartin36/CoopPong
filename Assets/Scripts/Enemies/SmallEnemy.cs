using System;
using System.Collections;
using UnityEngine;

public class SmallEnemy : BaseEnemy, IEffector {
	public StatusEffect Effect { get => StatusEffect.Shrunk; }

	public event EventHandler Destroyed;

	private Player targetedPlayer;

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
		var levelManager = GameManager.Instance.LevelManager;
		targetedPlayer = UnityEngine.Random.value > 0.5f ? levelManager.RightPlayer : levelManager.LeftPlayer;
		// target the computer if possible
		if (!targetedPlayer.OtherPlayer.PlayerControlled) {
			targetedPlayer = targetedPlayer.OtherPlayer;
		}
	}

	private void SpawnComplete() {
		Spawning = false;
		gameObject.layer = LayerMask.NameToLayer("Target");
		targetedPlayer.AddStatusEffect(this);
	}

	public void OnDestroy() {
		Destroyed?.Invoke(this, null);
	}
}