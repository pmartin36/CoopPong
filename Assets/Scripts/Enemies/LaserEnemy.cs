using UnityEngine;
using System.Collections;
using System;

public class LaserEnemy : MonoBehaviour, ISpawnable {
	public bool Spawning { get; set; }
	public LaserEnemyProperties Properties { get; set; }
	public Vector3 RestingPosition {
		get => Properties.RestingPosition;
		set => Properties.RestingPosition = value;
	}

	[SerializeField]
	private LaserShot LaserShotPrefab;

	public event EventHandler Destroyed;

	void Start() {

	}

	public void Init(SpawnProperties props) {
		Properties = props as LaserEnemyProperties;
		Spawning = true;

		transform.position = new Vector3(Properties.RestingPosition.x, 15f, 0);
	}

	void Update() {
		if (Spawning) {
			transform.position = transform.position.MoveTowards(RestingPosition, 3f * Time.deltaTime);
			if (Vector3.Distance(transform.position, RestingPosition) < 0.01f) {
				SpawnComplete();
			}
		}
		else {

		}
	}

	private void SpawnComplete() {
		Spawning = false;
		StartCoroutine(Fire());
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		Destroy(this.gameObject);
	}

	public void OnDestroy() {
		StopCoroutine(Fire());
		Destroyed?.Invoke(this, null);
	}

	private void CreateLaserShot(bool left) {
		LaserShot ls = Instantiate(LaserShotPrefab, null);
		if(left) {
			ls.transform.position = transform.position + Vector3.left;
			ls.Init( Vector3.left * 50f );
		} 
		else {
			ls.transform.position = transform.position + Vector3.right;
			ls.Init(Vector3.right * 50f);
		}
	}

	private IEnumerator Fire() {
		while(true) {
			yield return new WaitForSeconds(2f);
			CreateLaserShot(false);
			CreateLaserShot(true);
		}
	}
}
