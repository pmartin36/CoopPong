using UnityEngine;
using System.Collections;
using System;

public class LaserEnemy : BaseEnemy {
	[SerializeField]
	private LaserShot LaserShotPrefab;

	public event EventHandler Destroyed;

	private void OnEnable() {
		Spawning = true;
		StartCoroutine(SpawnOnDelay());
	}

	IEnumerator SpawnOnDelay() {
		yield return new WaitForSeconds(1f);
		SpawnComplete();
	}

	private void SpawnComplete() {
		Spawning = false;
		StartCoroutine(Fire());
	}

	public void OnDestroy() {
		StopCoroutine(Fire());
		Destroyed?.Invoke(this, null);
	}

	private void CreateLaserShot(bool left) {
		LaserShot ls = Instantiate(LaserShotPrefab, null);
		if(left) {
			ls.transform.position = transform.position + Vector3.left;
			ls.Init( Vector3.left * 50f, this.gameObject );
		} 
		else {
			ls.transform.position = transform.position + Vector3.right;
			ls.Init(Vector3.right * 50f, this.gameObject);
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
