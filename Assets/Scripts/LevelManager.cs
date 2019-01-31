using System;
using System.Collections;
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

	public int RemainingLives;
	public static event EventHandler<int> RemainingLivesChanged;	

	[SerializeField]
	private GameObject Floor;
	[SerializeField]
	private GameObject Ceiling;

	public override void Awake() {
		base.Awake();
		Camera c = Camera.main;

		//float size = c.orthographicSize; // TODO: Fix, don't use camera size - want to support something aside from 16:9?
		//LevelPlayableMinMax = new MinMax(-size + 0.5f, size); // TODO: Fix, don't hardcode

		Bounds box = Floor.GetComponent<BoxCollider2D>().bounds;
		LevelPlayableMinMax = new MinMax(box.min.y, box.max.y);
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

		SetRemainingLives(3);
	}

	public void Update() {
		
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
		SetRemainingLives(RemainingLives - 1);

		if(RemainingLives <= 0) {
			StartCoroutine(ExecuteDelayedAction(() => GameManager.Instance.ReloadLevel(), 1.2f));
		}
		else {
			// must come before or collider disable gets overriden
			LeftPlayer.SetInPlay(true);
			RightPlayer.SetInPlay(true);

			ball.SelectPlayerAndDropBall(false, true);		
		}
	}

	private void SetRemainingLives(int lives) {
		RemainingLives = lives;
		RemainingLivesChanged?.Invoke(this, RemainingLives);
	}

	public void LateUpdate() {
		
	}

	public IEnumerator ExecuteDelayedAction( Action action, float delay ) {
		yield return new WaitForSeconds(delay);
		action();
	}
}
