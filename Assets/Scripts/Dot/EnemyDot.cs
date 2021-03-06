﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDot : Dot
{
	public Transform[] Enemies;

	public override void Start() {
		base.Start();
		Enemies = GetComponentsInChildren<Transform>(true);
	}

	public override void OnDestroyEffect(GameObject ball) {
		// Spawn Enemy
		base.OnDestroyEffect(ball);
		foreach(Transform enemy in Enemies) {
			if(enemy != this.gameObject) {
				enemy.gameObject.SetActive(true);
				enemy.parent = transform.parent.parent; //Floor or Ceiling
			}
		}
	}
}
