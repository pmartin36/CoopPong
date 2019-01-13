using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteControlAttachment : MonoBehaviour
{
	public Ball ControlledBall { get; set; }
	private float TimeSinceLastCommunication = 0f;

    public void HandleInput(float vertical) {
		if( Time.time - TimeSinceLastCommunication > 0.5f || (ControlledBall != null && !ControlledBall.RemoteControlled) ) {
			float abs = Mathf.Abs(vertical);

			if(abs > 0.25f) {
				// if ball is moving towards player, flip input so it's consistent
				if ( (ControlledBall.MovementData.MovementDirection.x * transform.position.x) > 0 ) {
					vertical *= -1f;
				}
				vertical /= abs; // normalize
				ControlledBall?.RemoteControl(vertical);
				TimeSinceLastCommunication = Time.time;
			}
		}
	}
}
