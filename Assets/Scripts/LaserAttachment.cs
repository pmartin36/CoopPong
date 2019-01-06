using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserAttachment : MonoBehaviour
{
	public float Charge;
	public LaserShot LaserShotPrefab;
	private Player parentPlayer;
	private SpriteRenderer sr;

	private void Start() {
		sr = GetComponent<SpriteRenderer>();
	}

	private void OnEnable() {
		Fire();
	}

	public void Init(Player p) {
		parentPlayer = p;
	}

	private void Fire() {
		LaserShot ls = Instantiate(LaserShotPrefab, null);
		if (parentPlayer.Side == PlayerSide.Left) {
			ls.transform.position = transform.position;
			ls.Init(Vector3.right * 50f, parentPlayer.gameObject);
		}
		else {
			ls.transform.position = transform.position;
			ls.Init(Vector3.left * 50f, parentPlayer.gameObject);
		}
		Charge = 0;
	}

	public void Update() {
		Charge += Time.deltaTime / 5f;
		sr.color = Color.Lerp(Color.white, Color.red, Charge);
	}

	public bool TryFire() {
		if(Charge > 1) {
			Fire();
			return true;
		}
		return false;
	}
}
