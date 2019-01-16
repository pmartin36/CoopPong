using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserShot : MonoBehaviour
{
	private Vector3 MoveDirection;
	private float absMd;
	private TrailRenderer tr;
	public LayerMask mask;

	public bool DestroyOnHit;
	public bool CanCollectOrDestroy;
	
	private GameObject source;

	public void Start() {
		tr = GetComponentInChildren<TrailRenderer>();
	}

	public void Init(Vector3 direction, GameObject src) {
		MoveDirection = direction;
		absMd = MoveDirection.magnitude;

		source = src;
		this.gameObject.Destroy(2f);
	}

	public void Update() {
		var hit = Physics2D.CircleCastAll(transform.position, 0.25f, MoveDirection, absMd * Time.deltaTime, mask)
					.FirstOrDefault( h => h.collider != null && h.collider.gameObject != source && h.collider.gameObject != this.gameObject);
		if (hit.collider != null) {
			string tag = hit.collider.gameObject.tag;
			if (tag == "Player") {
				Player p = hit.collider.GetComponent<Player>();
				p.Hit(hit.point);
			}
			else if(CanCollectOrDestroy) {
				if(tag == "Enemy") {
					hit.collider.GetComponent<BaseEnemy>().TryDestroy();
				}
				else if(tag == "Dot") {
					hit.collider.GetComponent<Dot>().TryDestroy(this.gameObject);
				}
			}
			
			if(DestroyOnHit) {
				this.gameObject.Destroy();
			}
			transform.position = hit.centroid + (Vector2)MoveDirection.normalized * 0.25f;
		}
		else {
			transform.position += MoveDirection * Time.deltaTime;
		}
	}

	private void OnDestroy() {
		tr.transform.parent = null;
	}
}
