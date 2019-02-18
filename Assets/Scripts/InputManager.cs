using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputManager : MonoBehaviour
{
	protected void AddCommonControls(InputPackage p) {
		p.LeftPlayerBoost = Input.GetButton("LeftBoost");
		p.LeftPlayerCtrl = Input.GetButton("LeftCtrl");
		p.LeftPlayerVertical = Input.GetAxis("LeftVertical");
		p.LeftPlayerStart = Input.GetButtonDown("LeftStart");

		p.RightPlayerBoost = Input.GetButton("RightBoost");
		p.RightPlayerCtrl = Input.GetButton("RightCtrl");
		p.RightPlayerVertical = Input.GetAxis("RightVertical");
		p.RightPlayerStart = Input.GetButtonDown("RightStart");
	}
}
