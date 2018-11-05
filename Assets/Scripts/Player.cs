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
	private PlayerSide Side;

	private BoxCollider2D bodyCollider;

	private bool pipsOut;
	private float timeSinceLastEnergyUse;

	public float YMove {
		get { return MovementInput * MoveSpeed; }
	}

    // Start is called before the first frame update
    void Start()
    {
        ResetMoveSpeed();
		Side = transform.position.x > 0 ? PlayerSide.Right : PlayerSide.Left;
		Energy = 1;
		
		bodyCollider = GetComponentInChildren<BoxCollider2D>();
		var circleCollider = GetComponentInChildren<CircleCollider2D>();
		Width = (bodyCollider.size.y * bodyCollider.transform.lossyScale.y + circleCollider.radius * Mathf.Abs(circleCollider.transform.lossyScale.y) * 2);
		yMaximum = Camera.main.orthographicSize - (Width / 2f);
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

	//IEnumerator FlipSide() {
	//	float currentTime = 0f;
	//	isFlipping = true;
	//	float start = transform.localRotation.eulerAngles.z > 1f ? 180 : 0;
	//	float end = 180 - start;
	//	while(currentTime < 0.5f + Time.deltaTime) {
	//		transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(start, end, currentTime*2f));
	//		currentTime += Time.deltaTime;
	//		yield return new WaitForEndOfFrame();
	//	}
	//	isFlipping = false;
	//}
}
