using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInputManager : InputManager
{
    void Update()
    {
		InputPackage p = new InputPackage();
		AddCommonControls(p);
		GameManager.Instance.HandleInput(p);
	}
}
