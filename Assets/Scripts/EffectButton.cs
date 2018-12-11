using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ButtonLocation {
	TopLeft = 1,
	TopRight = 2,
	BottomLeft = 4,
	BottomRight = 8
}

public class EffectButton : MonoBehaviour
{
	public ButtonLocation buttonLocation;

	private bool isBottom;

	[SerializeField]
	private List<ButtonEffected> Effected;

	[SerializeField]
	private Player player;

	private void Start() {
		isBottom = buttonLocation == ButtonLocation.BottomLeft || buttonLocation == ButtonLocation.BottomRight;
	}

	private void Update() {
		float offset = player.Width / 2f;
		float y;
		bool active;
		if (isBottom) {
			y = Mathf.Clamp(player.transform.position.y - offset, -10, -9);
			active = y <= -9.5f;
		}
		else {
			y = Mathf.Clamp(player.transform.position.y + offset, 9, 10);
			active = y >= 9.5f;
		}
		transform.position = new Vector3(transform.position.x, y);

		if(active) {
			Pressed();
		}
	}

	public void Pressed() {
		foreach(var e in Effected) {
			e.ActedUponBy |= buttonLocation;
		}
	}
}
