using UnityEngine;
using System.Collections;
using System;

public class JailEnemy : MonoBehaviour, ISpawnable, IEffector {

	public bool Spawning { get; set; }
	public JailEnemyProperties Properties { get; set; }
	public Vector3 RestingPosition {
		get => Properties.RestingPosition;
		set => Properties.RestingPosition = value;
	}

	public StatusEffect Effect { get { return StatusEffect.Jailed; } }

	public event EventHandler Destroyed;

	private Player targetedPlayer;
	private static JailBar JailBarPrefab;

	public void Init(SpawnProperties props) {
		Properties = props as JailEnemyProperties;
		Spawning = true;

		var levelManager = GameManager.Instance.LevelManager;
		targetedPlayer = UnityEngine.Random.value > 0.5f ? levelManager.RightPlayer : levelManager.LeftPlayer;
		// target the computer if possible
		if (!targetedPlayer.OtherPlayer.PlayerControlled) {
			targetedPlayer = targetedPlayer.OtherPlayer;
		}

		transform.position = new Vector3(Properties.RestingPosition.x, 15f, 0);
	}

	public void Update() {
		if (Spawning) {
			transform.position = transform.position.MoveTowards(RestingPosition, 3f * Time.deltaTime); // not working
			if (Vector3.Distance(transform.position, RestingPosition) < 0.01f) {
				SpawnComplete();
			}
		}
		else {

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
		Destroy(this.gameObject);
	}

	public void OnDestroy() {
		Destroyed?.Invoke(this, null);
	}
}
