using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseCamera : MonoBehaviour
{
	private static Vector3 baseOffset;
	private Camera camera;

	public bool SwitchingPlayers { get; set; } = false;
	private HouseCameraPlayerFollower FollowedFollower;

    // Start is called before the first frame update
    void Awake()
    {
		baseOffset = new Vector3(12, 0, -20);
        camera = GetComponent<Camera>();
		HousePlayer.NewLeadPlayer += SetPlayer;
    }

	private void SetPlayer(object sender, EventArgs e) {
		FollowedFollower = (sender as HousePlayer).Follower;
		StartCoroutine(MoveToLeadPlayer());
	}

	void LateUpdate()
    {
		// position synced with follower
	}

	IEnumerator MoveToLeadPlayer() {
		SwitchingPlayers = true;
		float time = 0f;
		float jTime = 2f;
		while (time < jTime) {
			transform.position = Vector3.Lerp(transform.position, FollowedFollower.Position, time / jTime);
			transform.rotation = Quaternion.Lerp(transform.rotation, FollowedFollower.Rotation, time / jTime);
			time += Time.deltaTime;
			yield return null;
		}

		transform.position = FollowedFollower.Position;
		transform.rotation = FollowedFollower.Rotation;
		SwitchingPlayers = false;
	}
}
