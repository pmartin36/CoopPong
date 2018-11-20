using System;
using UnityEngine;

[Serializable]
public class SlowEnemyProperties : SpawnProperties {
	public Vector3 RestingPosition;

	public SlowEnemyProperties() { }
	public SlowEnemyProperties(Vector3 v) {
		RestingPosition = v;
	}
}

public class SlowEnemy : MonoBehaviour, IEffector, ISpawnable {

	public bool Spawning { get; set; }
	public SlowEnemyProperties Properties { get; set; }
	public Vector3 RestingPosition {
		get => Properties.RestingPosition;
		set => Properties.RestingPosition = value;
	}

	public StatusEffect Effect { get => StatusEffect.Slowed; }

	public event EventHandler Destroyed;

	private Player targetedPlayer;

	public void Init(SpawnProperties props) {
		Properties = props as SlowEnemyProperties;

		Spawning = true;

		var levelManager = GameManager.Instance.LevelManager;
		targetedPlayer = UnityEngine.Random.value > 0.5f ? levelManager.RightPlayer : levelManager.LeftPlayer;
		if (targetedPlayer.StatusEffects.HasFlag(Effect)) {
			targetedPlayer = targetedPlayer.OtherPlayer;
		}

		transform.position = new Vector3(Properties.RestingPosition.x, 15f, 0);
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