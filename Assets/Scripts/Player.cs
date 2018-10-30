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

	private float MovementInput;
	private float MoveSpeed;

	private static float yMaximum;
	private PlayerSide Side;

	public float YMove {
		get { return MovementInput * MoveSpeed; }
	}

    // Start is called before the first frame update
    void Start()
    {
        MoveSpeed = BaseSpeed;
		yMaximum = Camera.main.orthographicSize - (transform.localScale.y / 2f);
		Side = transform.position.x > 0 ? PlayerSide.Right : PlayerSide.Left;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void FixedUpdate() {
		float modifiedY = Mathf.Clamp(transform.position.y + MovementInput * MoveSpeed * Time.fixedDeltaTime, -yMaximum, yMaximum);
		transform.position = new Vector3(transform.position.x, modifiedY);
	}

	public void HandleInput(float vertical, float horizontal, bool horizontalDown) {
		MovementInput = vertical;
	}

	public Vector3 GetBallTrajectory(Vector3 point, Vector3 incoming) {
		float normalizedDistFromCenter = 2 * (point.y - transform.position.y) / transform.localScale.y; // -1 to 1
		
		Vector3 start = Side == PlayerSide.Right ? Vector3.left : Vector3.right;
		Vector3 end = new Vector3(0.7f * start.x, 0.7f * Mathf.Sign(normalizedDistFromCenter)).normalized;
		return Vector3.Lerp( start, end, Mathf.Pow(normalizedDistFromCenter,2));
	}
}
