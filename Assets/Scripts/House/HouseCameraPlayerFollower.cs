using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseCameraPlayerFollower : MonoBehaviour
{
	private static Vector3 baseOffset;
	static HouseCameraPlayerFollower() {
		baseOffset = new Vector3(-12, 0, -20);
	}

	public Vector3 Position { get => transform.position; }
	public Quaternion Rotation { get => transform.rotation; } 

	private float angleDiff {
		get {
			return Vector2.SignedAngle(
					transform.position - player.transform.position,
					-Utils.AngleToVector(player.transform.eulerAngles.z));
		}
	}

	[SerializeField]
	private HousePlayer player;
	private HouseCamera cam;

    // Start is called before the first frame update
    void Start()
    {
		cam = Camera.main.GetComponent<HouseCamera>();

		Vector3 rotated = Quaternion.Euler(0, 0, player.transform.eulerAngles.z) * baseOffset;
		transform.position = player.transform.position + rotated;
		transform.RotateAround(player.transform.position, Vector3.forward, this.angleDiff);

		player.Follower = this;
	}

    // Update is called once per frame
    void LateUpdate()
    {
		transform.position += player.MovementDelta;
		transform.RotateAround(player.transform.position, Vector3.forward, this.angleDiff * 0.015f);

		if(!cam.SwitchingPlayers && player.IsLeadPlayer) {
			cam.transform.position = this.transform.position;
			cam.transform.rotation = this.transform.rotation;
		}
	}
}
