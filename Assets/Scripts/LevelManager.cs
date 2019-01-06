using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : ContextManager
{
	public Player LeftPlayer;
	public Player RightPlayer;

	public Ball[] Balls;

	public SpawnExport SpawnDataAsset;

	// public Spawner Spawner;

	public bool IsSlowMode { get; private set; }

	public override void Start() {
		base.Start();
		// SpawnExport spawnData = Instantiate(SpawnDataAsset);
		// Spawner = new Spawner(spawnData.SpawnInfo);
		Balls = FindObjectsOfType<Ball>();
	}

	public void Update() {
		// Spawner.Update(Time.deltaTime);
	}

	public override void HandleInput(InputPackage p) {
		if(LeftPlayer != null) {
			LeftPlayer.HandleInput(p.LeftPlayerVertical, p.LeftPlayerFlip, p.LeftPlayerButton2);
		}

		if(RightPlayer != null) {
			RightPlayer.HandleInput(p.RightPlayerVertical, p.RightPlayerFlip, p.RightPlayerButton2);
		}
	}

	public bool SetSlowMode(bool slowMode) {
		if(IsSlowMode == slowMode) return false;

		IsSlowMode = slowMode;
		if(IsSlowMode) {
			GameManager.Instance.TimeScale = 0.5f;
			LeftPlayer.MaxMoveSpeed *= 2f;
			RightPlayer.MaxMoveSpeed *= 2f;
		}
		else {
			GameManager.Instance.TimeScale = 1f;
			LeftPlayer.ResetMoveSpeed();
			RightPlayer.ResetMoveSpeed();
		}

		return true;
	}

	public void PlayerLifeLost() {
		LeftPlayer.gameObject.SetActive(true);
		RightPlayer.gameObject.SetActive(true);
	}

	public void LateUpdate() {
		
	}
}
