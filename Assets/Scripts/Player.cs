using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlayerSide {
	Left,
	Right
}

public struct MinMax {
	public float Min { get; set; }
	public float Max { get; set; }

	public MinMax(float min, float max) : this() {
		Min = min;
		Max = max;
	}
}

public class Player : MonoBehaviour
{
	private Paddle paddle;
	private Character character;

	public float BaseSpeed;
	public float Width { get; private set; }
	public float BodyWidth { get => Width - 1.085f; }
	public float HalfLength { get => Width / 2f; }

	public bool IsSlowed { get; private set; }
	public bool IsControlling { get; private set; }

	public bool InPlay { get; private set; }

	public LaserAttachment Laser;
	public bool LaserActive {
		get { return Laser.isActiveAndEnabled; }
	}

	public RemoteControlAttachment RemoteControl;
	public bool RemoteControlActive {
		get { return RemoteControl.isActiveAndEnabled; }
	}

	public float MaxMoveSpeed { get; set; }
	private float targetMoveSpeed;
	private float lastFrameMoveSpeed;
	private float lastFrameAcceleration;

	private float? forcedMovementLocation;

	public AimAssist AimAssist;

	public MinMax MovementRange;
	public PlayerSide Side { get; set; }

	public StatusEffect StatusEffects;

	private bool pipsOut;
	private bool buttonDown;
	private float verticalInput;

	public bool PlayerControlled;
	// these fields only matter for cpu controlled player
	private float CpuBasePlacement;
	private float CpuOffsetPlacement;
	private bool CpuReachedBasePlacement;

	public float OffsetFromLine;
	public Ball LastBallInteractedWith { get; private set; }
	public bool CanPerformActions { get => InPlay && LastBallInteractedWith != null && !LastBallInteractedWith.Stopped; }

	[SerializeField]
	private List<GameObject> EffectedGameObjects;
	private List<IButtonEffected> Effected;

	[SerializeField]
	private Pet Pet;

	private CommandLocation UpCommand { get; set; }
	private CommandLocation DownCommand { get; set; }

	public float YMove { get => lastFrameMoveSpeed; }
	private float MovespeedSlow { get => IsSlowed ? 0.5f : 1f; }

	private Player _otherPlayer;
	public Player OtherPlayer {
		get {
			if(_otherPlayer == null) {
				var lm = GameManager.Instance.LevelManager;
				_otherPlayer = Side == PlayerSide.Left ? lm.RightPlayer : lm.LeftPlayer;
			}
			return _otherPlayer;
		}
	}

	private void OnValidate() {
		for (int i = 0; i < EffectedGameObjects.Count; i++) {
			if (EffectedGameObjects[i] != null && EffectedGameObjects[i].GetComponent<IButtonEffected>() == null) {
				EffectedGameObjects[i] = null;
			}
		}
	}

	void Start()
    {
        ResetMoveSpeed();
		Side = transform.position.x > 0 ? PlayerSide.Right : PlayerSide.Left;
		UpCommand = PlayerSide.Right == Side ? CommandLocation.UpRight : CommandLocation.UpLeft;
		DownCommand = PlayerSide.Right == Side ? CommandLocation.DownRight : CommandLocation.DownLeft;

		Effected = EffectedGameObjects.Select(g => g?.GetComponent<IButtonEffected>()).ToList();
		Laser = GetComponentInChildren<LaserAttachment>(true);
		RemoteControl = GetComponentInChildren<RemoteControlAttachment>(true);
		paddle = GetComponentInChildren<Paddle>();
		character = GetComponentInChildren<Character>();
	
		Laser.Init(paddle, Side);
		SetBodyWidth(1.15f, false);	
		paddle.Init(this);
		Pet.Init(UpCommand, DownCommand, Effected);

		transform.position = new Vector3(transform.position.x, UnityEngine.Random.Range(MovementRange.Min, MovementRange.Max));

		pipsOut = true;
		InPlay = true;
	}

	private void CalculateColor() {
		Color disabledColor = InPlay ? Color.white : new Color(0, 0, 0, 0.5f);
		Color baseColor = IsSlowed ? new Color(0.67f, 0.84f, 0.87f) : Color.white;

		paddle.SetColor(disabledColor);
		character.SetColor(baseColor);
	}

	void Update()
    {
        CalculateColor();
    }

	private void FixedUpdate() {
		if(PlayerControlled) {
			MoveFromInput(out bool overMaxSpeed);
		}
		else if (forcedMovementLocation == null) {
			float diff = CpuOffsetPlacement - transform.position.y;
			float diffAbs = Mathf.Abs(diff);
			float distance = Mathf.Min(diffAbs, (MaxMoveSpeed * MovespeedSlow) * Time.fixedDeltaTime);
			if(!CpuReachedBasePlacement && diffAbs < 0.05f) {
				CpuReachedBasePlacement = true;
			}

			float direction = Mathf.Sign(diff);
			var modifiedY = Mathf.Clamp(transform.position.y + direction * distance, MovementRange.Min, MovementRange.Max);
			transform.position = new Vector3(transform.position.x, modifiedY);
		}
	}

	private void MoveFromInput(out bool overMaxSpeed) {
		overMaxSpeed = false;
		if (buttonDown) {
			lastFrameMoveSpeed = 0;
			lastFrameAcceleration = 0;
		}
		else if(forcedMovementLocation == null) {
			float targetDelta = targetMoveSpeed; 
			lastFrameMoveSpeed = lastFrameMoveSpeed  + (lastFrameAcceleration * Time.fixedDeltaTime);
			if(pipsOut) {
				if( Mathf.Abs(lastFrameMoveSpeed) > MaxMoveSpeed ) {
					// we're slowing down after being in non-pips out mode
					targetDelta = Mathf.SmoothDamp(lastFrameMoveSpeed, targetDelta, ref lastFrameAcceleration, 0.3f);
					overMaxSpeed = true;
				}
				else if (Mathf.Abs(targetDelta) < Mathf.Abs(lastFrameMoveSpeed)) {
					targetDelta = Mathf.SmoothDamp(lastFrameMoveSpeed, targetDelta, ref lastFrameAcceleration, 0.1f);
				}
			}
			else {
				targetDelta *= 2.5f; // non-pips out can move at 2.5x speed
				bool slowingDown = Mathf.Abs(targetDelta) < Mathf.Abs(lastFrameMoveSpeed);
				targetDelta = Mathf.SmoothDamp(lastFrameMoveSpeed, targetDelta, ref lastFrameAcceleration, slowingDown ? 0.3f : 0.2f);
			}

			float modifiedY = Mathf.Clamp(transform.position.y + targetDelta * Time.fixedDeltaTime, MovementRange.Min, MovementRange.Max);
			lastFrameMoveSpeed = (modifiedY - transform.position.y) / Time.fixedDeltaTime;
			transform.position = new Vector3(transform.position.x, modifiedY);
		}
	}

	public void HandleInput(float vertical, bool launch, bool button1, bool button2) {
		verticalInput = vertical;
		if(PlayerControlled) {
			buttonDown = button2;
			if (buttonDown && CanPerformActions) {
				IsControlling = true;
				if(RemoteControlActive) {
					Pet.SetCommand(CommandLocation.None, 0);
					RemoteControl.HandleInput(vertical);
				}
				else {
					var vertAbs = Mathf.Abs(vertical);
					foreach (IButtonEffected e in Effected) {
						if (vertical > 0.1f) {
							Pet.SetCommand(UpCommand, vertAbs);
							// e.AddActor(UpCommand, vertAbs);
						}
						else if (vertical < -0.1f) {
							Pet.SetCommand(DownCommand, vertAbs);
							// e.AddActor(DownCommand, vertAbs);
						}
						else {
							Pet.Amount = 0;
						}
					}
				}
			}
			else {
				Pet.SetCommand(CommandLocation.None, 0);
				IsControlling = false;
				targetMoveSpeed = vertical * MaxMoveSpeed * MovespeedSlow;

				if(LastBallInteractedWith != null && LastBallInteractedWith.Stopped && launch && OffsetFromLine < 0.01f) {
					// allow push out
					StartCoroutine(PushBall());
				}

				if (button1) {
					if(!LaserActive) {
						SetPipsOut(!pipsOut);
					}
					else if(CanPerformActions) {
						Laser.TryFire();
					}
				}
			}
		} 
	}

	private void SetPipsOut(bool pips) {
		pipsOut = pips;
		paddle.SetPips(pips);
	}

	public void ResetMoveSpeed() {
		MaxMoveSpeed = BaseSpeed;
	}

	public void PlaceBall(Ball b) {
		LastBallInteractedWith = b;
		b.transform.position = paddle.transform.position + new Vector3(Side == PlayerSide.Left ? 1f : -1f, 0);
	}

	public Vector3 GetBallTrajectory(Ball b, Vector3 point, Vector3 incoming) {
		// take this opportunity to add ball to remote controller
		LastBallInteractedWith = b;
		RemoteControl.ControlledBall = b;

		// calculate new trajectory
		float normalizedDistFromCenter = 2 * (point.y - transform.position.y) / Width; // -1 to 1
		
		Vector3 start = Side == PlayerSide.Right ? Vector3.left : Vector3.right;

		float angle = Mathf.Lerp(0, 60, Mathf.Abs(normalizedDistFromCenter));
		angle *= Mathf.Sign(normalizedDistFromCenter);
		Vector3 end = Utils.AngleToVector(angle);
		end.x *= start.x;
		
		return Vector3.Lerp( start, end, Mathf.Pow(normalizedDistFromCenter,2));
	}

	public float GetRotationModifier(Vector3 point, Vector3 incoming) {
		return (!pipsOut || LaserActive)  ? 0.1f : 2f;
	}

	public float GetMSDelta(Vector3 point, Vector3 incoming) {
		return (pipsOut && !LaserActive) ? -0.1f : 0.25f;
	}

	public void GoToLocation(float py) {
		CpuReachedBasePlacement = false;
		CpuBasePlacement = py;

		float range = Width * 0.475f;
		CpuOffsetPlacement = CpuBasePlacement + UnityEngine.Random.Range(-range, range );
	}

	public void SetBodyWidth(float bodyWidth, bool animate = true) {
		bodyWidth = Mathf.Clamp(bodyWidth, 0.25f, 2.5f);
		// will animate in the future
		animate = false;
		if(animate) {

		}
		else {		
			//0.5425 = height of end caps
			Width = (bodyWidth + 0.5425f / 2f) * 2;

			MinMax levelMinMax = GameManager.Instance.LevelManager.LevelPlayableMinMax;
			MovementRange = new MinMax(levelMinMax.Min + HalfLength, levelMinMax.Max - HalfLength);

			paddle.SetWidth(Width, bodyWidth);
		}
	}

	public void AddPowerup(Powerup p) {
		switch (p) {
			case Powerup.BigBall:
				foreach(Ball b in GameManager.Instance.LevelManager.Balls) {
					b.ApplyBigBall();
				}
				break;
			case Powerup.Laser:
				if(RemoteControlActive) {
					RemoteControl.gameObject.SetActive(false);
				}
				Laser.gameObject.SetActive(true);
				SetPipsOut(true);
				break;
			case Powerup.Remote:
				if (LaserActive) {
					Laser.gameObject.SetActive(false);
				}
				RemoteControl.gameObject.SetActive(true);
				break;
		}
	}

	public void ApplyMoveSlow() {
		StopCoroutine("Slow");
		StartCoroutine("Slow");
	}

	public void AddStatusEffect(IEffector effector) {
		StatusEffects &= effector.Effect;
		effector.Destroyed += EffectorDestroyed;

		switch (effector.Effect) {
			case StatusEffect.Shrunk:
				SetBodyWidth( BodyWidth - 0.5f );
				break;
			case StatusEffect.Jailed:
				MinMax levelMinMax = GameManager.Instance.LevelManager.LevelPlayableMinMax;
				MovementRange = new MinMax(
					Mathf.Max(levelMinMax.Min + HalfLength, transform.position.y - 3f),
					Mathf.Min(levelMinMax.Max - HalfLength, transform.position.y + 3f)
				);
				break;
			case StatusEffect.Blinded:
				break;
			default:
				break;
		}
	}

	public void EffectorDestroyed(object sender, EventArgs e) {
		StatusEffect effectRemoved = (sender as IEffector).Effect;
		StatusEffects &= ~effectRemoved;

		switch (effectRemoved) {
			case StatusEffect.Shrunk:
				SetBodyWidth(BodyWidth + 0.5f);
				break;
			case StatusEffect.Jailed:
				if(GameManager.Instance?.LevelManager?.LevelPlayableMinMax != null) {
					MinMax levelMinMax = GameManager.Instance.LevelManager.LevelPlayableMinMax;
					MovementRange = new MinMax(levelMinMax.Min + HalfLength, levelMinMax.Max - HalfLength);
				}		
				break;
			case StatusEffect.Blinded:
				break;
			default:
				break;
		}
	}

	public void Hit(Vector3 position) {
		if(!LastBallInteractedWith.Stopped) {
			IsSlowed = false;
			SetInPlay(false);

			if(position.y < transform.position.y) {
				forcedMovementLocation = position.y + (Width + 0.5f);
			}
			else {
				forcedMovementLocation = position.y - (Width + 0.5f);
			}

			if(!PlayerControlled) {
				CpuOffsetPlacement = forcedMovementLocation.Value;
				CpuReachedBasePlacement = false;
			}
			StartCoroutine(JumpFromLaser());
		}
	}

	public void SetInPlay(bool inplay) {	
		InPlay = inplay;
		paddle.SetInPlay(inplay);
	}

	private IEnumerator Slow() {
		IsSlowed = true;
		yield return new WaitForSeconds(4f);
		IsSlowed = false;
	}

	private IEnumerator JumpFromLaser() {
		float startTime = Time.time;
		float start = transform.position.y;
		float end = forcedMovementLocation.Value;
		var wait = new WaitForEndOfFrame();
		while(Time.time - startTime < 0.25f) {
			transform.position = new Vector3(transform.position.x, Mathf.Lerp(start, end, (Time.time - startTime) / 0.25f));
			yield return wait;
		}
		forcedMovementLocation = null;
	}

	private IEnumerator PushBall() {
		float startTime = Time.time;
		float pushedPosition = Side == PlayerSide.Right ? -1f : 1f;
		float baseline = Paddle.BaseLinePosition * (Side == PlayerSide.Left ? -1f : 1f);
		var wait = new WaitForEndOfFrame();

		while (Time.time - startTime < 0.25f) {
			OffsetFromLine = Mathf.Lerp(0, pushedPosition, (Time.time - startTime) / 0.25f);
			paddle.transform.position = new Vector3(baseline + OffsetFromLine, transform.position.y, paddle.transform.position.z);
			yield return wait;
		}

		startTime = Time.time;
		while (Time.time - startTime < 0.35f) {
			OffsetFromLine = Mathf.Lerp(pushedPosition, 0, (Time.time - startTime) / 0.25f);
			paddle.transform.position = new Vector3(baseline + OffsetFromLine, transform.position.y, paddle.transform.position.z);
			yield return wait;
		}
	}
}
