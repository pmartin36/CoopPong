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
	public float BaseSpeed;
	public float Width { get; private set; }
	public float BodyWidth {
		get {
			return (Width / 2f) - 0.25f;
		}
	}

	public bool SoulSwapped { get; set; } = false;

	public float Energy { get; private set; }
	public bool SlowModeActive { get; private set; }

	private float MovementInput;
	public float MaxMoveSpeed { get; set; }
	private float LastFrameMoveSpeed;

	public AimAssist AimAssist;

	private static float yMaximum;
	public MinMax MovementRange;
	public PlayerSide Side { get; set; }

	public StatusEffect StatusEffects;

	private bool pipsOut;
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

	public float YMove {
		get { return MovementInput * MaxMoveSpeed; }
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

    void Start()
    {
        ResetMoveSpeed();
		Side = transform.position.x > 0 ? PlayerSide.Right : PlayerSide.Left;
		Energy = 1;

		spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

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

		AimAssist = AimAssist.Light;
	}

	void Update()
    {
        if(Energy < 1 && Time.time - timeSinceLastEnergyUse > 1f) {
			Energy += 0.5f * Time.deltaTime; 
		}
    }

	private void FixedUpdate() {
		if(PlayerControlled) {
			MoveFromInput(MovementInput);
		}
		else {
			float diff = CpuOffsetPlacement - transform.position.y;
			float diffAbs = Mathf.Abs(diff);
			float distance = Mathf.Min(diffAbs, MaxMoveSpeed * Time.fixedDeltaTime);
			if(!CpuReachedBasePlacement && diffAbs < 0.05f) {
				CpuReachedBasePlacement = true;
			}

			float direction = Mathf.Sign(diff);
			var modifiedY = Mathf.Clamp(transform.position.y + direction * distance, MovementRange.Min, MovementRange.Max);
			transform.position = new Vector3(transform.position.x, modifiedY);
		}
	}

	private void MoveFromInput(float mInput) {
		float delta = mInput * MaxMoveSpeed * Time.fixedDeltaTime;
		float modifiedY = Mathf.Clamp(transform.position.y + delta, MovementRange.Min, MovementRange.Max);
		transform.position = new Vector3(transform.position.x, modifiedY);
	}

	public void HandleInput(float vertical, bool flip, bool slow) {
		if(PlayerControlled) {
			MovementInput = vertical;
			if(flip) {
				transform.localRotation = Quaternion.Euler(0, 0, transform.localRotation.eulerAngles.z + 180);
				pipsOut = !pipsOut;
			}
	
			if (slow && (Energy > 0.99f || SlowModeActive)) {
				SlowModeActive = true;
				Energy -= 0.75f * Time.deltaTime;
				timeSinceLastEnergyUse = Time.time;
			}
			else {
				SlowModeActive = false;
			}
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
		return pipsOut ? 2f : 1f;
	}

	public float GetMSDelta(Vector3 point, Vector3 incoming) {
		return pipsOut ? -0.1f : 0.25f;
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
			Width = (bodyWidth + 0.25f) * 2;
			yMaximum = Camera.main.orthographicSize - (Width / 2) - 0.25f;
			MovementRange = new MinMax(-yMaximum, yMaximum);

			capsuleCollider.size = new Vector2(capsuleCollider.size.x, Width);
			body.localScale = new Vector2(body.localScale.x, bodyWidth);
			topCap.localPosition = new Vector2(0, bodyWidth);
			bottomCap.localPosition = new Vector2(0, -bodyWidth);
		}
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
		this.gameObject.SetActive(false);
	}
}
