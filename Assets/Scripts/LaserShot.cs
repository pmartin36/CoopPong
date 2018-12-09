using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShot : MonoBehaviour
{
	private Vector3 MoveDirection;
	[SerializeField]
	private LayerMask mask;

	public void Init(Vector3 direction) {
		MoveDirection = direction;
		this.gameObject.Destroy(2f);
	}

	public void Update() {
		var hit = Physics2D.CircleCast(transform.position, 0.5f, MoveDirection, MoveDirection.x * Time.deltaTime, mask);
		if (hit.collider != null) {
			Player p = hit.collider.GetComponent<Player>();
			p.Hit();
		}

		transform.position += MoveDirection * Time.deltaTime;
	}
}
