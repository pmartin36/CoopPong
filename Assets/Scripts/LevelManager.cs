using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LevelInputManager))]
public class LevelManager : ContextManager
{
	public Player LeftPlayer;
	public Player RightPlayer;

	public Ball[] Balls;

	private int _numActiveBalls;
	public int NumActiveBalls { get => _numActiveBalls; set => _numActiveBalls = Mathf.Max(0, value); }

	public readonly List<Player> CpuControlledPlayers = new List<Player>();

	public bool IsSlowMode { get; private set; }

	public int RemainingLives;
	public static event EventHandler<int> RemainingLivesChanged;	

	public GameObject Floor;
	public GameObject Ceiling;

	public PlayingField PlayingField;
	public MinMax LevelPlayableMinMax { get => this.PlayingField.MinMax; }

	private int RequiredDotCount = -1;
	private bool GoldenDotCollected;

	private Camera camera;
	private Vector3 cameraSittingPosition;

	private Animator anim;

	public override void Awake() {
		base.Awake();
		camera = Camera.main;
		anim = GetComponent<Animator>();
		PlayingField = FindObjectOfType<PlayingField>();
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

	public override void HandleInput(InputPackage p) {
		if(LeftPlayer != null) {
			LeftPlayer.HandleInput(p.LeftPlayerVertical, p.LeftPlayerStart, p.LeftPlayerBoost, p.LeftPlayerCtrl);
		}

		if(RightPlayer != null) {
			RightPlayer.HandleInput(p.RightPlayerVertical, p.RightPlayerStart, p.RightPlayerBoost, p.RightPlayerCtrl);
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

	public void CeilingSwitchEnd() {
		Transform levelHolder = Ceiling.transform.parent;
		levelHolder.rotation = Quaternion.Euler(180, 0, 0);
		PlayingField.CalculateBoundaries();

		camera.transform.position = new Vector3(0, 3, -25);
		camera.transform.rotation = Quaternion.Euler(9,0,0);

		anim.enabled = false;
		Destroy(Floor);

		foreach(Ball b in Balls) {
			b.gameObject.SetActive(true);
			b.SelectPlayerAndDropBall(true);
		}
		LeftPlayer.CeilingSwitchEnd();
		RightPlayer.CeilingSwitchEnd();
	}

	public void AddRequiredDot() {
		if(RequiredDotCount < 0) {
			RequiredDotCount = 1;
		}
		else {
			RequiredDotCount++;
		}
	}

	public void DotCollected(DotType d) {
		if(d == DotType.Required) {
			RequiredDotCount--;
			if(RequiredDotCount <= 0) {
				foreach(Ball b in Balls) {
					b.gameObject.SetActive(false);
				}
				if(Ceiling.activeInHierarchy) {
					// end level
					StartCoroutine(ExecuteDelayedAction(() => GameManager.Instance.ReloadLevel(), 1.2f));
				}
				else {
					// go to ceiling
					anim.enabled = true;
					RightPlayer.CeilingSwitchStart();
					LeftPlayer.CeilingSwitchStart();
				}
			}
		}
		else if(d == DotType.Golden) {
			GoldenDotCollected = true;
		}
	}

	public IEnumerator ExecuteDelayedAction( Action action, float delay ) {
		yield return new WaitForSeconds(delay);
		action();
	}
}
