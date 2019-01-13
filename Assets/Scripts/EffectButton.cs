using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CommandLocation {
	None = 0,
	TopLeft = 1,
	TopRight = 2,
	BottomLeft = 4,
	BottomRight = 8
}

public class EffectButton : MonoBehaviour
{
	public CommandLocation buttonLocation;

	private bool isBottom;

	[SerializeField]
	private List<GameObject> EffectedGameObjects;
	private List<IButtonEffected> Effected;

	[SerializeField]
	private Player player;

	private void OnValidate() {
		for(int i = 0; i < EffectedGameObjects.Count; i++) {
			if( EffectedGameObjects[i]?.GetComponent<IButtonEffected>() == null ){
				EffectedGameObjects[i] = null;
			}
		}
	}

	private void Start() {
		isBottom = buttonLocation == CommandLocation.BottomLeft || buttonLocation == CommandLocation.BottomRight;
		Effected = EffectedGameObjects.Select( g => g.GetComponent<IButtonEffected>() ).ToList();
	}

	private void Update() {
		float offset = player.Width / 2f;
		float y;
		if (isBottom) {
			y = Mathf.Clamp(player.transform.position.y - offset, -10, -9);
			Pressed(-y - 9f);
		}
		else {
			y = Mathf.Clamp(player.transform.position.y + offset, 9, 10);
			Pressed(y - 9f);
		}
		transform.position = new Vector3(transform.position.x, y);
	}

	public void Pressed(float amount) {
		//foreach(var e in Effected) {
		//	e.AddActor(this.buttonLocation, amount);
		//}
	}
}
