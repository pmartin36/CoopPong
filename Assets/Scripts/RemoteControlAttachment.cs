using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteControlAttachment : MonoBehaviour
{
	public Ball ControlledBall { get; set; }

    public void HandleInput(float vertical) {
		// if ball is moving towards player, flip input so it's consistent
		if( (ControlledBall.MovementData.MovementDirection.x * transform.position.x) > 0 ) {
			vertical *= -1f;
		}
		ControlledBall?.RemoteControl(vertical);
	}
}
