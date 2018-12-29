using System;
using System.Collections;
using UnityEngine;

public class SmallEnemy : MonoBehaviour, IEffector {

	public bool Spawning { get; set; }

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

	public void Update() {
		
	}

	private void SpawnComplete() {
		Spawning = false;
		gameObject.layer = LayerMask.NameToLayer("Target");
		targetedPlayer.AddStatusEffect(this);
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		if (!Spawning) {
			Destroy(this.gameObject);
		}
	}

	public void OnDestroy() {
		Destroyed?.Invoke(this, null);
	}
}