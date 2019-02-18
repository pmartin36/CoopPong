using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseCamera : MonoBehaviour
{
	private static Vector3 offset;
	private Camera camera;
	private HousePlayer leadPlayer;

    // Start is called before the first frame update
    void Awake()
    {
		offset = new Vector3(12, 0, -20);
        camera = GetComponent<Camera>();
		HousePlayer.NewLeadPlayer += SetPlayer;
    }

	private void SetPlayer(object sender, EventArgs e) {
		leadPlayer = sender as HousePlayer;
		transform.position = leadPlayer.transform.position + offset;
	}

	void LateUpdate()
    {
		transform.position += leadPlayer.MovementDelta;

		// var e = transform.eulerAngles;
		// transform.RotateAround(leadPlayer.transform.position, Vector3.forward, leadPlayer.RotationDelta);
		var angleDiff = Vector2.SignedAngle(
			transform.position - leadPlayer.transform.position,
			-Utils.AngleToVector(leadPlayer.transform.eulerAngles.z));

		transform.RotateAround(leadPlayer.transform.position, Vector3.forward, angleDiff * 0.015f);
	}
}
