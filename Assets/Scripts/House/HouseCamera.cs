using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseCamera : MonoBehaviour
{
	private static Vector3 baseOffset;
	private Camera camera;
	private HousePlayer leadPlayer;

	public bool SwitchingPlayers { get; set; } = false;

    // Start is called before the first frame update
    void Awake()
    {
		baseOffset = new Vector3(12, 0, -20);
        camera = GetComponent<Camera>();
		HousePlayer.NewLeadPlayer += SetPlayer;
    }

	private void SetPlayer(object sender, EventArgs e) {
		leadPlayer = sender as HousePlayer;
		// transform.position = leadPlayer.transform.position + baseOffset;
		StartCoroutine(MoveToLeadPlayer());
	}

	void LateUpdate()
    {
		if(leadPlayer != null) {
			if(!SwitchingPlayers) {
				transform.position += leadPlayer.MovementDelta;
			}

			// var e = transform.eulerAngles;
			// transform.RotateAround(leadPlayer.transform.position, Vector3.forward, leadPlayer.RotationDelta);
			var angleDiff = Vector2.SignedAngle(
				transform.position - leadPlayer.transform.position,
				-Utils.AngleToVector(leadPlayer.transform.eulerAngles.z));

			transform.RotateAround(leadPlayer.transform.position, Vector3.forward, angleDiff * 0.015f);
		}
	}

	IEnumerator MoveToLeadPlayer() {
		SwitchingPlayers = true;
		float dist = 1;

		while (dist > 0.1f) {
			Vector3 targetOffset = Quaternion.Euler(0, 0, leadPlayer.transform.eulerAngles.z) * baseOffset;

			var angleDiff = Vector2.SignedAngle(
				transform.position - leadPlayer.transform.position,
				-Utils.AngleToVector(leadPlayer.transform.eulerAngles.z));

			// transform.LookAt(leadPlayer.transform, Vector3.forward);

			Vector3 targetPosition = leadPlayer.transform.position + targetOffset;
			Vector3 diff = (targetPosition - transform.position);
			dist = diff.sqrMagnitude;

			transform.position += diff;
			dist -= dist;
			yield return null;
		}

		transform.position = leadPlayer.transform.position + baseOffset;
		SwitchingPlayers = false;
	}
}
