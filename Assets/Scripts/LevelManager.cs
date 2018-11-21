using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : ContextManager
{
	public Player LeftPlayer;
	public Player RightPlayer;

	public SpawnExport SpawnData;

	public Spawner Spawner;

	public bool IsSlowMode { get; private set; }

	public override void Start() {
		base.Start();
		Spawner = new Spawner(SpawnData.SpawnInfo);
	}

	public void Update() {
		Spawner.Update(Time.deltaTime);
	}

	public override void HandleInput(InputPackage p) {
		if(LeftPlayer != null) {
			LeftPlayer.HandleInput(p.LeftPlayerVertical, p.LeftPlayerFlip, p.LeftPlayerSlow);
		}

		if(RightPlayer != null) {
			RightPlayer.HandleInput(p.RightPlayerVertical, p.RightPlayerFlip, p.RightPlayerSlow);
		}
	}

	public bool SetSlowMode(bool slowMode) {
		if(IsSlowMode == slowMode) return false;

		IsSlowMode = slowMode;
		if(IsSlowMode) {
			GameManager.Instance.TimeScale = 0.5f;
			LeftPlayer.MoveSpeed *= 2f;
			RightPlayer.MoveSpeed *= 2f;
		}
		else {
			GameManager.Instance.TimeScale = 1f;
			LeftPlayer.ResetMoveSpeed();
			RightPlayer.ResetMoveSpeed();
		}

		return true;
	}

	public void LateUpdate() {
		SetSlowMode(LeftPlayer.SlowModeActive || RightPlayer.SlowModeActive);
	}
}
