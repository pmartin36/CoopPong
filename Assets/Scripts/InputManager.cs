using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPackage {
	public bool LeftPlayerFlip { get; set; }
	public bool LeftPlayerButton2 { get; set; }
	public bool LeftPlayerLaunchBall { get; set; }
	public float LeftPlayerVertical { get; set; }
	
	public bool RightPlayerFlip { get; set; }
	public bool RightPlayerButton2 { get; set; }
	public bool RightPlayerLaunchBall { get; set; }
	public float RightPlayerVertical { get; set; }
}

public class InputManager : MonoBehaviour
{
    void Update()
    {
        InputPackage p = new InputPackage();
		p.LeftPlayerFlip = Input.GetButtonDown("LeftButton1");
		p.LeftPlayerButton2 = Input.GetButton("LeftButton2");
		p.LeftPlayerVertical = Input.GetAxis("LeftVertical");
		p.LeftPlayerLaunchBall = Input.GetButtonDown("LeftLaunchBall");

		p.RightPlayerFlip = Input.GetButtonDown("RightButton1");
		p.RightPlayerButton2 = Input.GetButton("RightButton2");
		p.RightPlayerVertical = Input.GetAxis("RightVertical");
		p.RightPlayerLaunchBall = Input.GetButtonDown("RightLaunchBall");

		GameManager.Instance.HandleInput(p);
	}
}
