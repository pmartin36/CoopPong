using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPackage {
	public bool LeftPlayerHorizontalDown { get; set; }
	public float LeftPlayerHorizontal { get; set; }
	public float LeftPlayerVertical { get; set; }
	
	public bool RightPlayerHorizontalDown { get; set; }
	public float RightPlayerHorizontal { get; set; }
	public float RightPlayerVertical { get; set; }
}

public class InputManager : MonoBehaviour
{
    void Update()
    {
        InputPackage p = new InputPackage();
		p.LeftPlayerHorizontalDown = Input.GetButtonDown("LeftHorizontal");
		p.LeftPlayerHorizontal = Input.GetAxis("LeftHorizontal");
		p.LeftPlayerVertical = Input.GetAxis("LeftVertical");

		p.RightPlayerHorizontalDown = Input.GetButtonDown("RightHorizontal");
		p.RightPlayerHorizontal = Input.GetAxis("RightHorizontal");
		p.RightPlayerVertical = Input.GetAxis("RightVertical");

		GameManager.Instance.HandleInput(p);
	}
}
