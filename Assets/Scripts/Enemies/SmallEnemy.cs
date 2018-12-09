using System;
using UnityEngine;

public class SmallEnemy : MonoBehaviour, IEffector, ISpawnable {

	public bool Spawning { get; set; }
	public SmallEnemyProperties Properties { get; set; }
	public Vector3 RestingPosition {
		get => Properties.RestingPosition;
		set => Properties.RestingPosition = value;
	}

	public StatusEffect Effect { get => StatusEffect.Shrunk; }

	public event EventHandler Destroyed;

	private Player targetedPlayer;

	public void Init(SpawnProperties props) {
		Properties = props as SmallEnemyProperties;

		Spawning = true;

		SelectTarget();
		transform.position = new Vector3(Properties.RestingPosition.x, 15f, 0);
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
		if( Spawning ) {
			transform.position = transform.position.MoveTowards(RestingPosition, 3f * Time.deltaTime); // not working
			if( Vector3.Distance(transform.position, RestingPosition) < 0.01f ) {
				SpawnComplete();
			}
		}
		else {

		}
	}

	private void SpawnComplete() {
		Spawning = false;
		gameObject.layer = LayerMask.NameToLayer("Target");
		targetedPlayer.AddStatusEffect(this);
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		Destroy(this.gameObject);
	}

	public void OnDestroy() {
		Destroyed?.Invoke(this, null);
	}
}