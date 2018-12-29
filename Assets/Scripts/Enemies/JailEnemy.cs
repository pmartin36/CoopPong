using UnityEngine;
using System.Collections;
using System;

public class JailEnemy : MonoBehaviour, IEffector {

	public bool Spawning { get; set; }

	public StatusEffect Effect { get { return StatusEffect.Jailed; } }

	public event EventHandler Destroyed;

	private Player targetedPlayer;
	private static JailBar JailBarPrefab;

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

		var oldMinMax = targetedPlayer.MovementRange;
		targetedPlayer.AddStatusEffect(this);

		JailBarPrefab = JailBarPrefab ?? Resources.Load<JailBar>("Prefabs/Jail Bar");
		if (targetedPlayer.MovementRange.Min > oldMinMax.Min) {
			JailBar b = Instantiate(JailBarPrefab);
			b.transform.position = new Vector3(targetedPlayer.transform.position.x, targetedPlayer.MovementRange.Min - (targetedPlayer.Width + b.transform.lossyScale.y) / 2f);
			Destroyed += b.JailerDestroyed;
		}

		if(targetedPlayer.MovementRange.Max < oldMinMax.Max){
			JailBar b = Instantiate(JailBarPrefab);
			b.transform.position = new Vector3(targetedPlayer.transform.position.x, targetedPlayer.MovementRange.Max + (targetedPlayer.Width + b.transform.lossyScale.y) / 2f);
			Destroyed += b.JailerDestroyed;
		}	
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
