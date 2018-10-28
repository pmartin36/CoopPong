using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : ContextManager
{
	public Player LeftPlayer;
	public Player RightPlayer;

	public override void HandleInput(InputPackage p) {
		if(LeftPlayer != null) {
			LeftPlayer.HandleInput(p.LeftPlayerVertical, p.LeftPlayerHorizontal, p.LeftPlayerHorizontalDown);
		}

		if(RightPlayer != null) {
			RightPlayer.HandleInput(p.RightPlayerVertical, p.RightPlayerHorizontal, p.RightPlayerHorizontalDown);
		}
	}
}
