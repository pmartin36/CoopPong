using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseEnemy: MonoBehaviour {
	public bool Spawning { get; set; }

	public bool TryDestroy() {
		if(!Spawning) {
			this.gameObject.Destroy();
			return true;
		}
		return false;
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		TryDestroy();
	}
}

