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

	public float YMove {
		get { return MovementInput * MoveSpeed; }
	}

	public Player OtherPlayer {
		get {
			var lm = GameManager.Instance.LevelManager;
			return Side == PlayerSide.Left ? lm.RightPlayer : lm.LeftPlayer;
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
		float modifiedY = Mathf.Clamp(transform.position.y + MovementInput * MoveSpeed * Time.fixedDeltaTime, -yMaximum, yMaximum);
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
		MoveSpeed = BaseSpeed;
	}

	public Vector3 GetBallTrajectory(Vector3 point, Vector3 incoming) {
		float normalizedDistFromCenter = 2 * (point.y - transform.position.y) / Width; // -1 to 1
		
		Vector3 start = Side == PlayerSide.Right ? Vector3.left : Vector3.right;

		float angle = Mathf.Lerp(0, 60, Mathf.Abs(normalizedDistFromCenter));
		angle *= Mathf.Sign(normalizedDistFromCenter);
		Vector3 end = Utils.AngleToVector(angle);
		end.x *= start.x;
		// Debug.Log(end);

		//Vector3 end = new Vector3(0.7f * start.x, 0.7f * Mathf.Sign(normalizedDistFromCenter)).normalized;
		return Vector3.Lerp( start, end, Mathf.Pow(normalizedDistFromCenter,2));
	}

	public float GetRotationModifier(Vector3 point, Vector3 incoming) {
		return pipsOut ? 2f : 1f;
	}

	public float GetMSDelta(Vector3 point, Vector3 incoming) {
		return pipsOut ? -0.1f : 0.25f;
	}

	public void GoToLocation(Vector3 p) {
		if(PlayerMoveCoroutine != null) {
			StopCoroutine(PlayerMoveCoroutine);
		}
		p = new Vector3(transform.position.x, Mathf.Clamp(p.y + Random.Range(-1.2f,1.2f), -yMaximum, yMaximum));
		PlayerMoveCoroutine = StartCoroutine(MoveToLocation(p));
	}

	private IEnumerator MoveToLocation(Vector3 p) {
		float toGo = Mathf.Abs(p.y - transform.position.y);
		Vector3 direction = Vector3.up * Mathf.Sign(p.y - transform.position.y);
		while ( toGo > 0.1f ) {
			float toMove = MoveSpeed * Time.deltaTime;
			if( toGo > toMove ) {
				transform.position += direction * toMove;
			}
			else {
				transform.position += direction * toGo;
				yield break;
			}
			toGo = Mathf.Abs(p.y - transform.position.y);
			yield return new WaitForEndOfFrame();
		}
	}
}
