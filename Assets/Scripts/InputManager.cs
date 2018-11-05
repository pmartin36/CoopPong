using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPackage {
	public bool LeftPlayerFlip { get; set; }
	public bool LeftPlayerSlow { get; set; }
	public float LeftPlayerVertical { get; set; }
	
	public bool RightPlayerFlip{ get; set; }
	public bool RightPlayerSlow { get; set; }
	public float RightPlayerVertical { get; set; }
}

public class InputManager : MonoBehaviour
{
    void Update()
    {
        InputPackage p = new InputPackage();
		p.LeftPlayerFlip = Input.GetButtonDown("LeftFlip");
		p.LeftPlayerSlow = Input.GetButton("LeftSlow");
		p.LeftPlayerVertical = Input.GetAxis("LeftVertical");

		p.RightPlayerFlip = Input.GetButtonDown("RightFlip");
		p.RightPlayerSlow = Input.GetButton("RightSlow");
		p.RightPlayerVertical = Input.GetAxis("RightVertical");

		GameManager.Instance.HandleInput(p);
	}
}
