﻿using System;
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
	public float BaseSpeed;
	public float Width { get; private set; }
	public float BodyWidth {
		get {
			return (Width / 2f) - 0.25f;
		}
	}

	public float Energy { get; private set; }
	public bool IsSlowed { get; private set; }
	public bool IsControlling { get; private set; }

	public LaserAttachment Laser;
	public bool LaserActive {
		get { return Laser.isActiveAndEnabled; }
	}

	public float MaxMoveSpeed { get; set; }
	private float targetMoveSpeed;
	private float lastFrameMoveSpeed;
	private float lastFrameAcceleration;

	public AimAssist AimAssist;

	private static float yMaximum;
	public MinMax MovementRange;
	public PlayerSide Side { get; set; }

	public StatusEffect StatusEffects;

	private bool pipsOut;
	private bool buttonDown;
	private float verticalInput;
	private float timeSinceLastEnergyUse;
	
	private CapsuleCollider2D capsuleCollider;
	private SpriteRenderer[] spriteRenderers;
	private Transform body;
	private Transform topCap;
	private Transform bottomCap;

	public bool PlayerControlled;
	// these fields only matter for cpu controlled player
	private float CpuBasePlacement;
	private float CpuOffsetPlacement;
	private bool CpuReachedBasePlacement;

	[SerializeField]
	private List<GameObject> EffectedGameObjects;
	private List<IButtonEffected> Effected;

	public float YMove {
		get { return lastFrameMoveSpeed; }
	}

	private float MovespeedSlow {
		get {
			return IsSlowed ? 0.5f : 1f;
		}
	}

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
		Energy = 1;

		spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

		Laser = GetComponentInChildren<LaserAttachment>(true);
		Laser.Init(this);

		foreach(Transform t in transform) {
			if(t.tag == "PlayerCapTop") {
				topCap = t;
			}
			else if(t.tag == "PlayerBody") {
				body = t;
			}
			else if(t.tag == "PlayerCapBottom") {
				bottomCap = t;
			}	
		}

		capsuleCollider = GetComponent<CapsuleCollider2D>();
		SetBodyWidth(1.15f, false);

		Effected = EffectedGameObjects.Select(g => g?.GetComponent<IButtonEffected>()).ToList();
		pipsOut = true;
	}

	private void CalculateColor() {
		Color baseColor = IsSlowed ? new Color(0.67f, 0.84f, 0.87f) : Color.white;
		foreach (SpriteRenderer s in spriteRenderers) {
			if(!IsControlling) {
				s.color = baseColor;
			}
			else {
				if (verticalInput > 0 && s.transform == topCap) {
					s.color = baseColor * Color.red;
				}
				else if (verticalInput < 0 && s.transform == bottomCap) {
					s.color = baseColor * Color.red;
				}
				else {
					s.color = baseColor * Color.yellow;
				}
			}
		}		
	}

	void Update()
    {
        CalculateColor();
    }

	private void FixedUpdate() {
		if(PlayerControlled) {
			MoveFromInput(out bool overMaxSpeed);
		}
		else {
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
		else{
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

	public void HandleInput(float vertical, bool button1, bool button2) {
		verticalInput = vertical;
		if(PlayerControlled) {
			buttonDown = button2;
			if (buttonDown) {
				IsControlling = true;
				var vertAbs = Mathf.Abs(vertical);
				foreach (IButtonEffected e in Effected) {
					if (Side == PlayerSide.Right) {
						if (vertical > 0.5f) {
							e.AddActor(ButtonLocation.TopRight, vertAbs);
						}
						else if (vertical < -0.5f) {
							e.AddActor(ButtonLocation.BottomRight, vertAbs);
						}
					}
					else {
						if (vertical > 0.5f) {
							e.AddActor(ButtonLocation.TopLeft, vertAbs);
						}
						else if (vertical < -0.5f) {
							e.AddActor(ButtonLocation.BottomLeft, vertAbs);
						}
					}
				}
			}
			else {
				IsControlling = false;
				targetMoveSpeed = vertical * MaxMoveSpeed * MovespeedSlow;
				if (button1) {
					if(LaserActive) {
						Laser.TryFire();
					}
					else {
						SetPipsOut(!pipsOut);			
					}
				}
			}
		} 
	}

	private void SetPipsOut(bool pips) {
		pipsOut = pips;
		foreach (SpriteRenderer s in spriteRenderers) {
			s.flipX = pips;
		}
	}

	public void ResetMoveSpeed() {
		MaxMoveSpeed = BaseSpeed;
	}

	public Vector3 GetBallTrajectory(Vector3 point, Vector3 incoming) {
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

	public void GoToLocation(Vector3 p) {
		CpuReachedBasePlacement = false;
		CpuBasePlacement = p.y;

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
			yMaximum = Camera.main.orthographicSize - (Width / 2) - 0.5425f / 2f;
			MovementRange = new MinMax(-yMaximum, yMaximum);

			capsuleCollider.size = new Vector2(capsuleCollider.size.x, Width);
			body.localScale = new Vector2(body.localScale.x, bodyWidth);
			topCap.localPosition = new Vector2(0, bodyWidth);
			bottomCap.localPosition = new Vector2(0, -bodyWidth);
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
				Laser.gameObject.SetActive(true);
				SetPipsOut(true);
				break;
			case Powerup.Remote:
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
				MovementRange = new MinMax(
					Mathf.Max(-yMaximum, transform.position.y - 3f),
					Mathf.Min(yMaximum, transform.position.y + 3f)
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
				MovementRange = new MinMax(-yMaximum, yMaximum);
				break;
			case StatusEffect.Blinded:
				break;
			default:
				break;
		}
	}

	public void Hit() {
		IsSlowed = false;
		this.gameObject.SetActive(false);
	}

	private IEnumerator Slow() {
		IsSlowed = true;
		yield return new WaitForSeconds(4f);
		IsSlowed = false;
	}
}
