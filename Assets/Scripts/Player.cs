using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerSide {
	Left,
	Right
}

public class Player : MonoBehaviour
{
	public float BaseSpeed;
	public float Width { get; private set; }
	public float Energy { get; private set; }
	public bool SlowModeActive { get; private set; }

	private float MovementInput;
	public float MoveSpeed { get; set; }

	private static float yMaximum;
	public PlayerSide Side { get; set; }

	private CapsuleCollider2D capCollider;

	private bool pipsOut;
	private float timeSinceLastEnergyUse;

	public AimAssist AimAssist { get; set; }

	public bool PlayerControlled;
	private Coroutine PlayerMoveCoroutine;

	// these fields only matter for cpu controlled player
	private Vector3 CpuBasePlacement;
	private Vector3 CpuOffsetPlacement;
	private bool CpuReachedBasePlacement;

	public float YMove {
		get { return MovementInput * MoveSpeed; }
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

    // Start is called before the first frame update
    void Start()
    {
        ResetMoveSpeed();
		Side = transform.position.x > 0 ? PlayerSide.Right : PlayerSide.Left;
		Energy = 1;

		capCollider = GetComponent<CapsuleCollider2D>();
		Width = capCollider.size.y * transform.lossyScale.y;
		yMaximum = Camera.main.orthographicSize - (Width / 2f);

		AimAssist = AimAssist.Heavy;
	}

    // Update is called once per frame
    void Update()
    {
        if(Energy < 1 && Time.time - timeSinceLastEnergyUse > 1f) {
			Energy += 0.5f * Time.deltaTime; 
		}
    }

	private void FixedUpdate() {
		if(PlayerControlled) {
			float modifiedY = Mathf.Clamp(transform.position.y + MovementInput * MoveSpeed * Time.fixedDeltaTime, -yMaximum, yMaximum);
			transform.position = new Vector3(transform.position.x, modifiedY);
		}
		else {
			float ms = CpuReachedBasePlacement ? BaseSpeed / 5f : BaseSpeed;
			float diff = CpuOffsetPlacement.y - transform.position.y;
			float diffAbs = Mathf.Abs(diff);
			float distance = Mathf.Min(diffAbs, ms * Time.fixedDeltaTime);
			if(diffAbs < 0.05f) {
				CpuReachedBasePlacement = true;
			}

			Vector3 direction = Vector3.up * Mathf.Sign(diff);
			transform.position += direction * distance;
		}
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

			//if(!OtherPlayer.PlayerControlled && OtherPlayer.CpuReachedBasePlacement) {
			//	OtherPlayer.SetCpuOffsetPlacement( OtherPlayer.CpuOffsetPlacement.y + vertical * Time.deltaTime );
			//}
		} 
	}

	private void SetCpuOffsetPlacement(float diffFromBase) {
		diffFromBase = Mathf.Clamp(diffFromBase, -1.2f, 1.2f);
		CpuOffsetPlacement = new Vector3(CpuBasePlacement.x, Mathf.Clamp(CpuBasePlacement.y + diffFromBase, -yMaximum, yMaximum));
	}

	public void ResetMoveSpeed() {
		MoveSpeed = BaseSpeed;
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
		CpuBasePlacement = new Vector3(transform.position.x, p.y);
		SetCpuOffsetPlacement(Random.Range(-1.2f, 1.2f));
	}
}
