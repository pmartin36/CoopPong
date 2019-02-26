using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseCameraPlayerFollower : MonoBehaviour
{
	private static Vector3 baseOffset;
	static HouseCameraPlayerFollower() {
		baseOffset = new Vector3(12, 0, -20);
	}

	[SerializeField]
	private HousePlayer player;
	private HouseCamera cam;

    // Start is called before the first frame update
    void Start()
    {
		cam = Camera.main.GetComponent<HouseCamera>();
        transform.position = player.transform.position + baseOffset;

		var angleDiff = Vector2.SignedAngle(
			transform.position - player.transform.position,
			-Utils.AngleToVector(player.transform.eulerAngles.z));

		transform.RotateAround(player.transform.position, Vector3.forward, angleDiff);

		player.Follower = this;
	}

    // Update is called once per frame
    void LateUpdate()
    {
		transform.position += player.MovementDelta;

		var angleDiff = Vector2.SignedAngle(
			transform.position - player.transform.position,
			-Utils.AngleToVector(player.transform.eulerAngles.z));

		transform.RotateAround(player.transform.position, Vector3.forward, angleDiff * 0.015f);

		if(!cam.SwitchingPlayers && player.IsLeadPlayer) {
			cam.transform.position = this.transform.position;
			cam.transform.rotation = this.transform.rotation;
		}
	}
}
