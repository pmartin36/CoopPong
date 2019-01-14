using UnityEngine;
using System.Collections;
using System;

public class JailEnemy : BaseEnemy, IEffector {

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

		MinMax oldMinMax = targetedPlayer.MovementRange;
		targetedPlayer.AddStatusEffect(this);

		MinMax levelMinMax = GameManager.Instance.LevelManager.LevelPlayableMinMax;

		JailBarPrefab = JailBarPrefab ?? Resources.Load<JailBar>("Prefabs/Jail Bar");
		if (targetedPlayer.MovementRange.Min > oldMinMax.Min) {
			JailBar b = Instantiate(JailBarPrefab);
			float scale = (targetedPlayer.MovementRange.Min - targetedPlayer.Width / 2f) - levelMinMax.Min;
			float position = levelMinMax.Min + scale / 2f;
			b.transform.position = new Vector3(targetedPlayer.transform.position.x * 1.05f, position);
			b.transform.localScale = new Vector3(b.transform.localScale.x, scale);
			b.SignUpForDestroyedEvent(this);
		}

		if(targetedPlayer.MovementRange.Max < oldMinMax.Max){
			JailBar b = Instantiate(JailBarPrefab);
			float scale = levelMinMax.Max - (targetedPlayer.MovementRange.Max + targetedPlayer.Width / 2f);
			float position = levelMinMax.Max - scale / 2f;
			b.transform.position = new Vector3(targetedPlayer.transform.position.x * 1.05f, position);
			b.transform.localScale = new Vector3(b.transform.localScale.x, scale);
			b.SignUpForDestroyedEvent(this);
		}	
	}

	public void OnDestroy() {
		Destroyed?.Invoke(this, null);
	}
}
