using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDot : Dot
{
	public Transform[] Enemies;

	public void Start() {
		Enemies = GetComponentsInChildren<Transform>(true);
	}

	public override void OnDestroyEffect() {
		// Spawn Enemy
		foreach(Transform enemy in Enemies) {
			if(enemy != this.gameObject) {
				enemy.gameObject.SetActive(true);
				enemy.parent = null;
			}
		}
	}
}
