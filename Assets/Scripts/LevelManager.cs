﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : ContextManager
{
	public Player LeftPlayer;
	public Player RightPlayer;

	public Ball[] Balls;

	private int _numActiveBalls;
	public int NumActiveBalls { get => _numActiveBalls; set => _numActiveBalls = Mathf.Max(0, value); }

	public readonly List<Player> CpuControlledPlayers = new List<Player>();

	public SpawnExport SpawnDataAsset;

	public MinMax LevelPlayableMinMax;

	public bool IsSlowMode { get; private set; }

	public override void Awake() {
		base.Awake();
		Camera c = Camera.main;
		float size = c.orthographicSize; // TODO: Fix, don't use camera size - want to support something aside from 16:9?
		LevelPlayableMinMax = new MinMax(-size + 0.5f, size); // TODO: Fix, don't hardcode
	}

	public override void Start() {
		base.Start();
		Balls = FindObjectsOfType<Ball>();

		if(!LeftPlayer.PlayerControlled) {
			CpuControlledPlayers.Add(LeftPlayer);
		}
		if(!RightPlayer.PlayerControlled) {
			CpuControlledPlayers.Add(RightPlayer);
		}	
	}

	public void Update() {
		// Spawner.Update(Time.deltaTime);
	}

	public override void HandleInput(InputPackage p) {
		if(LeftPlayer != null) {
			LeftPlayer.HandleInput(p.LeftPlayerVertical, p.LeftPlayerLaunchBall, p.LeftPlayerFlip, p.LeftPlayerButton2);
		}

		if(RightPlayer != null) {
			RightPlayer.HandleInput(p.RightPlayerVertical, p.RightPlayerLaunchBall, p.RightPlayerFlip, p.RightPlayerButton2);
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

	public void PlayerLifeLost(Ball ball) {
		LeftPlayer.SetInPlay(true);
		RightPlayer.SetInPlay(true);
	}

	public void LateUpdate() {
		
	}
}
