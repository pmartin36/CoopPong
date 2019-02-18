using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseInputManager : InputManager
{
	void Update() {
		HouseInputPackage p = new HouseInputPackage();
		p.LeftPlayerHorizontal = Input.GetAxis("LeftHorizontal");
		p.RightPlayerHorizontal = Input.GetAxis("RightHorizontal");
		AddCommonControls(p);
		GameManager.Instance.HandleInput(p);
	}
}
